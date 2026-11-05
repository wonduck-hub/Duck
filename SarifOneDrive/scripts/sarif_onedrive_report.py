
#!/usr/bin/env python3
"""Upload original SARIF files to project-specific OneDrive folders."""

import argparse
import json
import os
import re
import subprocess
import sys
import io
import time
import hashlib
from collections import Counter
from openpyxl import Workbook
from openpyxl.chart import LineChart, Reference
from openpyxl.styles import Font, PatternFill, Alignment
from openpyxl.utils import get_column_letter

from datetime import datetime, timezone
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, quote
from urllib.request import Request, urlopen


AUTHORITY = "https://login.microsoftonline.com/consumers"
TOKEN_ENDPOINT = AUTHORITY + "/oauth2/v2.0/token"
GRAPH_API = "https://graph.microsoft.com/v1.0"

SARIF_FOLDER = "SARIF"
GRAPH_SCOPE = "Files.ReadWrite offline_access"
SECRET_NAME = "ONEDRIVE_REFRESH_TOKEN"
SUPPORTED_SARIF_VERSION = "2.1.0"


# --------------------------------------------------
# SARIF
# --------------------------------------------------

def read_sarif(sarif_path):
    with open(sarif_path, "rb") as sarif_file:
        sarif_bytes = sarif_file.read()

    try:
        sarif = json.loads(sarif_bytes.decode("utf-8-sig"))
    except (UnicodeDecodeError, json.JSONDecodeError) as error:
        raise ValueError(
            "Input must be a valid UTF-8 SARIF document"
        ) from error

    if (
        not isinstance(sarif, dict)
        or sarif.get("version") != SUPPORTED_SARIF_VERSION
    ):
        raise ValueError(
            "Input must use SARIF version {}".format(
                SUPPORTED_SARIF_VERSION
            )
        )

    if not isinstance(sarif.get("runs"), list):
        raise ValueError(
            "SARIF document must contain a runs array"
        )

    return sarif_bytes


def normalize_project_name(project_name):
    safe_name = re.sub(
        r"[^A-Za-z0-9._-]+",
        "-",
        project_name
    )

    safe_name = safe_name.strip(".-_")

    if not safe_name:
        raise ValueError("Invalid project name")

    return safe_name


def make_filename(project_name, timestamp):
    return "{}_sarif_{}.sarif".format(
        project_name,
        timestamp
    )


# --------------------------------------------------
# Microsoft authentication
# --------------------------------------------------

def post_form(url, form):
    request = Request(
        url,
        data=urlencode(form).encode("utf-8"),
        headers={
            "Content-Type": "application/x-www-form-urlencoded"
        },
        method="POST"
    )

    with urlopen(request, timeout=30) as response:
        return json.loads(response.read().decode("utf-8"))


def refresh_access_token(client_id, refresh_token):
    try:
        token_response = post_form(
            TOKEN_ENDPOINT,
            {
                "client_id": client_id,
                "grant_type": "refresh_token",
                "refresh_token": refresh_token,
                "scope": GRAPH_SCOPE
            }
        )
    except HTTPError as error:
        raise RuntimeError(
            "Microsoft refresh-token request failed with HTTP {}".format(
                error.code
            )
        ) from error
    except URLError as error:
        raise RuntimeError(
            "Microsoft refresh-token request failed"
        ) from error
    except (json.JSONDecodeError, UnicodeDecodeError) as error:
        raise RuntimeError(
            "Microsoft returned an invalid token response"
        ) from error

    access_token = token_response.get("access_token")

    if not isinstance(access_token, str) or not access_token:
        raise RuntimeError(
            "Microsoft token response did not include an access token"
        )

    rotated_refresh_token = token_response.get("refresh_token")

    if rotated_refresh_token is not None and (
        not isinstance(rotated_refresh_token, str)
        or not rotated_refresh_token
    ):
        raise RuntimeError(
            "Microsoft returned an invalid rotated refresh token"
        )

    return access_token, rotated_refresh_token


# --------------------------------------------------
# Microsoft Graph
# --------------------------------------------------

def graph_request(
    access_token,
    url,
    method="GET",
    body=None
):
    request_data = None

    if body is not None:
        request_data = json.dumps(body).encode("utf-8")

    request = Request(
        url,
        data=request_data,
        headers={
            "Authorization": "Bearer " + access_token,
            "Content-Type": "application/json"
        },
        method=method
    )

    try:
        with urlopen(request, timeout=30) as response:
            response_body = response.read()

            if not response_body:
                return response.status, {}

            return (
                response.status,
                json.loads(response_body.decode("utf-8"))
            )

    except HTTPError:
        raise
    except URLError as error:
        raise RuntimeError(
            "Microsoft Graph request failed"
        ) from error


# --------------------------------------------------
# OneDrive folders
# --------------------------------------------------

def ensure_folder(access_token, parent_path, folder_name):
    if parent_path:
        full_path = parent_path + "/" + folder_name

        parent_url = (
            GRAPH_API
            + "/me/drive/root:/"
            + quote(parent_path, safe="/")
            + ":/children"
        )
    else:
        full_path = folder_name
        parent_url = GRAPH_API + "/me/drive/root/children"

    folder_url = (
        GRAPH_API
        + "/me/drive/root:/"
        + quote(full_path, safe="/")
    )

    try:
        _, folder = graph_request(
            access_token,
            folder_url
        )

    except HTTPError as error:
        if error.code != 404:
            raise RuntimeError(
                "Could not check OneDrive folder (HTTP {})".format(
                    error.code
                )
            ) from error

        try:
            _, folder = graph_request(
                access_token,
                parent_url,
                method="POST",
                body={
                    "name": folder_name,
                    "folder": {},
                    "@microsoft.graph.conflictBehavior": "fail"
                }
            )

        except HTTPError as create_error:
            if create_error.code != 409:
                raise RuntimeError(
                    "Could not create OneDrive folder (HTTP {})".format(
                        create_error.code
                    )
                ) from create_error

            # Another request may have created the folder.
            _, folder = graph_request(
                access_token,
                folder_url
            )

    if (
        not isinstance(folder, dict)
        or not isinstance(folder.get("folder"), dict)
    ):
        raise RuntimeError(
            "OneDrive path is not a folder: {}".format(
                full_path
            )
        )

    return folder


def ensure_sarif_folders(access_token, project_name):
    ensure_folder(
        access_token,
        "",
        SARIF_FOLDER
    )

    ensure_folder(
        access_token,
        SARIF_FOLDER,
        project_name
    )


# --------------------------------------------------
# Upload original SARIF
# --------------------------------------------------

def upload_sarif(
    access_token,
    project_name,
    filename,
    sarif_bytes
):
    upload_path = "{}/{}/{}".format(
        SARIF_FOLDER,
        project_name,
        filename
    )

    upload_url = (
        GRAPH_API
        + "/me/drive/root:/"
        + quote(upload_path, safe="/")
        + ":/content"
    )

    request = Request(
        upload_url,
        data=sarif_bytes,
        headers={
            "Authorization": "Bearer " + access_token,
            "Content-Type": "application/json"
        },
        method="PUT"
    )

    try:
        with urlopen(request, timeout=60) as response:
            if response.status not in (200, 201):
                raise RuntimeError(
                    "OneDrive upload failed with HTTP {}".format(
                        response.status
                    )
                )

    except HTTPError as error:
        raise RuntimeError(
            "OneDrive upload failed with HTTP {}".format(
                error.code
            )
        ) from error

    except URLError as error:
        raise RuntimeError(
            "OneDrive upload request failed"
        ) from error


# --------------------------------------------------
# GitHub refresh token rotation
# --------------------------------------------------

def update_github_refresh_secret(repository, refresh_token):
    gh_token = os.environ.get(
        "GH_SECRETS_PAT",
        ""
    ).strip()

    if not gh_token:
        raise ValueError(
            "GH_SECRETS_PAT is required to persist the rotated refresh token"
        )

    environment = os.environ.copy()
    environment["GH_TOKEN"] = gh_token

    try:
        result = subprocess.run(
            [
                "gh",
                "secret",
                "set",
                SECRET_NAME,
                "--repo",
                repository
            ],
            input=refresh_token,
            text=True,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            check=False,
            env=environment,
            timeout=60
        )

    except (OSError, subprocess.TimeoutExpired) as error:
        raise RuntimeError(
            "Could not update the GitHub refresh-token secret"
        ) from error

    if result.returncode != 0:
        raise RuntimeError(
            "Could not update the GitHub refresh-token secret; verify PAT permissions"
        )


# The JSON ledger is the authoritative history; workbooks are generated views.
LEDGER_PATH = SARIF_FOLDER + '/quality_history.json'
MAX_LEDGER_RETRIES = 8


def graph_binary(token, path, method='GET', data=None, etag=None):
    url = GRAPH_API + '/me/drive/root:/' + quote(path, safe='/') + ':/content'
    headers = {'Authorization': 'Bearer ' + token}
    if data is not None:
        headers['Content-Type'] = 'application/octet-stream'
    if etag:
        headers['If-Match'] = etag
    req = Request(url, data=data, headers=headers, method=method)
    for attempt in range(5):
        try:
            with urlopen(req, timeout=90) as response:
                return response.read()
        except HTTPError as exc:
            if exc.code in (429, 500, 502, 503, 504) and attempt < 4:
                delay = exc.headers.get('Retry-After', '')
                time.sleep(min(int(delay), 30) if delay.isdigit() else 2 ** attempt)
                continue
            raise


def get_item(token, path):
    url = GRAPH_API + '/me/drive/root:/' + quote(path, safe='/')
    try:
        return graph_request(token, url)[1]
    except HTTPError as exc:
        if exc.code == 404:
            return None
        raise


def read_ledger(token):
    item = get_item(token, LEDGER_PATH)
    if item is None:
        return {'schema': 1, 'entries': []}, None
    data = json.loads(graph_binary(token, LEDGER_PATH).decode('utf-8-sig'))
    if data.get('schema') != 1 or not isinstance(data.get('entries'), list):
        raise ValueError('Unsupported quality_history.json schema')
    return data, item.get('eTag')


def parse_metrics(sarif):
    counts = Counter()
    rules = Counter()
    files = Counter()
    tools = set()
    for run in sarif['runs']:
        driver = (run.get('tool') or {}).get('driver') or {}
        if driver.get('name'):
            tools.add(driver['name'] + (' ' + driver['version'] if driver.get('version') else ''))
        rule_levels = {r.get('id'): (r.get('defaultConfiguration') or {}).get('level', 'warning')
                       for r in driver.get('rules', []) if isinstance(r, dict)}
        for result in run.get('results', []):
            rule = result.get('ruleId') or (result.get('rule') or {}).get('id') or '(unknown)'
            level = result.get('level') or rule_levels.get(rule, 'warning')
            if level not in ('error', 'warning', 'note', 'none'):
                level = 'warning'
            counts[level] += 1
            rules[rule] += 1
            locations = result.get('locations') or []
            if locations:
                loc = (locations[0].get('physicalLocation') or {}).get('artifactLocation') or {}
                uri = loc.get('uri')
                if uri:
                    files[uri] += 1
    return {'error': counts['error'], 'warning': counts['warning'],
            'note': counts['note'], 'none': counts['none'],
            'total': sum(counts.values()), 'rules': dict(rules),
            'files': dict(files), 'tools': sorted(tools)}


def create_entry(project, filename, timestamp, repository, commit, sarif):
    raw = json.dumps(sarif, sort_keys=True, ensure_ascii=False).encode('utf-8')
    # GitHub run attempt distinguishes reruns while preserving deduplication on retries.
    run_id = os.environ.get('GITHUB_RUN_ID', '')
    attempt = os.environ.get('GITHUB_RUN_ATTEMPT', '1')
    identity = '|'.join([repository, project, commit, run_id, attempt, hashlib.sha256(raw).hexdigest()])
    return {'id': hashlib.sha256(identity.encode()).hexdigest(), 'project': project,
            'timestamp': timestamp, 'repository': repository, 'commit': commit,
            'sarif': filename, **parse_metrics(sarif)}


def save_ledger(token, entry):
    for attempt in range(MAX_LEDGER_RETRIES):
        ledger, etag = read_ledger(token)
        if any(row.get('id') == entry['id'] for row in ledger['entries']):
            return ledger
        ledger['entries'].append(entry)
        ledger['entries'].sort(key=lambda row: (row['timestamp'], row['project'], row['id']))
        data = json.dumps(ledger, ensure_ascii=False, indent=2).encode('utf-8')
        try:
            if etag is None:
                # First writer creates the ledger; races retry after 412/409.
                url = GRAPH_API + '/me/drive/root:/' + quote(LEDGER_PATH, safe='/') + ':/content'
                req = Request(url, data=data, method='PUT', headers={
                    'Authorization': 'Bearer ' + token,
                    'Content-Type': 'application/json', 'If-None-Match': '*'})
                with urlopen(req, timeout=90):
                    pass
            else:
                graph_binary(token, LEDGER_PATH, 'PUT', data, etag)
            return ledger
        except HTTPError as exc:
            if exc.code in (409, 412) and attempt < MAX_LEDGER_RETRIES - 1:
                time.sleep(min(0.5 * (attempt + 1), 4))
                continue
            raise
    raise RuntimeError('Could not update ledger after concurrent modifications')


def style_sheet(ws, widths):
    ws.sheet_view.showGridLines = False
    ws.freeze_panes = 'A2'
    ws.auto_filter.ref = ws.dimensions
    for cell in ws[1]:
        cell.fill = PatternFill('solid', fgColor='17365D')
        cell.font = Font(color='FFFFFF', bold=True)
        cell.alignment = Alignment(vertical='center')
    ws.row_dimensions[1].height = 24
    for col, width in enumerate(widths, 1):
        ws.column_dimensions[get_column_letter(col)].width = width
    for row in ws.iter_rows(min_row=2):
        for cell in row:
            cell.font = Font(color='00804A' if isinstance(cell.value, (int, float)) else '444444')


def add_sheet(wb, name, headers, rows, widths):
    ws = wb.create_sheet(name)
    ws.append(headers)
    for row in rows:
        ws.append(row)
    style_sheet(ws, widths)
    return ws


def workbook_bytes(entries, project=None):
    rows = sorted((e for e in entries if project is None or e['project'] == project),
                  key=lambda e: (e['timestamp'], e['project'], e['id']))
    wb = Workbook()
    wb.remove(wb.active)
    headers = ['UTC Timestamp', 'Project', 'Repository', 'Commit SHA', 'Error',
               'Warning', 'Note', 'None', 'Total', 'Tools', 'SARIF File']
    history = add_sheet(wb, 'History', headers,
                        [[e['timestamp'], e['project'], e['repository'], e['commit'],
                          e['error'], e['warning'], e['note'], e['none'], e['total'],
                          ', '.join(e.get('tools', [])), e['sarif']] for e in rows],
                        [23, 23, 34, 43, 12, 12, 12, 12, 12, 34, 52])
    by_project = {}
    for e in rows:
        by_project[e['project']] = e
    add_sheet(wb, 'Latest', headers,
              [[e['timestamp'], e['project'], e['repository'], e['commit'],
                e['error'], e['warning'], e['note'], e['none'], e['total'],
                ', '.join(e.get('tools', [])), e['sarif']]
               for _, e in sorted(by_project.items())],
              [23, 23, 34, 43, 12, 12, 12, 12, 12, 34, 52])
    rule_rows = []
    file_rows = []
    for e in rows:
        for rule, count in sorted(e.get('rules', {}).items()):
            rule_rows.append([e['timestamp'], e['project'], e['commit'], rule, count])
        for file, count in sorted(e.get('files', {}).items()):
            file_rows.append([e['timestamp'], e['project'], e['commit'], file, count])
    add_sheet(wb, 'Rules', ['UTC Timestamp', 'Project', 'Commit SHA', 'Rule ID', 'Count'],
              rule_rows, [23, 23, 43, 38, 12])
    add_sheet(wb, 'Files', ['UTC Timestamp', 'Project', 'Commit SHA', 'File URI', 'Count'],
              file_rows, [23, 23, 43, 90, 12])
    if rows:
        chart = LineChart()
        chart.title = 'Findings by analysis run'
        chart.y_axis.title = 'Count'
        chart.x_axis.title = 'Run (UTC)'
        for column in (5, 6, 7):
            chart.add_data(Reference(history, min_col=column, min_row=1,
                                     max_row=history.max_row), titles_from_data=True)
        chart.set_categories(Reference(history, min_col=1, min_row=2,
                                       max_row=history.max_row))
        chart.width = 22
        chart.height = 11
        history.add_chart(chart, 'M2')
    buffer = io.BytesIO()
    wb.save(buffer)
    return buffer.getvalue()


def upload_reports(token, ledger, project):
    entries = ledger['entries']
    graph_binary(token, f'{SARIF_FOLDER}/{project}/{project}_Quality.xlsx', 'PUT',
                 workbook_bytes(entries, project))
    graph_binary(token, f'{SARIF_FOLDER}/AllProjects_Quality.xlsx', 'PUT',
                 workbook_bytes(entries))


def run(sarif_path, project_name):
    client_id = os.environ.get('MS_CLIENT_ID', '').strip()
    refresh_token = os.environ.get('ONEDRIVE_REFRESH_TOKEN', '').strip()
    repository = os.environ.get('GITHUB_REPOSITORY', '').strip()
    commit = os.environ.get('GITHUB_SHA', '').strip()
    if not all((client_id, refresh_token, repository, commit)):
        raise ValueError('MS_CLIENT_ID, ONEDRIVE_REFRESH_TOKEN, GITHUB_REPOSITORY and GITHUB_SHA are required')
    sarif_bytes = read_sarif(sarif_path)
    sarif = json.loads(sarif_bytes.decode('utf-8-sig'))
    project = normalize_project_name(project_name)
    timestamp = os.environ.get('SARIF_TIMESTAMP', '').strip() or datetime.now(timezone.utc).strftime('%Y%m%d-%H%M%S')
    if not re.fullmatch(r'\d{8}-\d{6}', timestamp):
        raise ValueError('SARIF_TIMESTAMP must be YYYYMMDD-HHMMSS')
    filename = make_filename(project, timestamp)
    run_id = os.environ.get('GITHUB_RUN_ID', '').strip()
    run_attempt = os.environ.get('GITHUB_RUN_ATTEMPT', '1').strip()
    if run_id:
        if not run_id.isdigit() or not run_attempt.isdigit():
            raise ValueError('Invalid GitHub run identifiers')
        filename = make_filename(project, f'{timestamp}_{run_id}_{run_attempt}')
    token, rotated = refresh_access_token(client_id, refresh_token)
    # Preserve the refreshed credential immediately, even if a later upload fails.
    if rotated and rotated != refresh_token:
        update_github_refresh_secret(repository, rotated)
    ensure_sarif_folders(token, project)
    upload_sarif(token, project, filename, sarif_bytes)
    entry = create_entry(project, filename, timestamp, repository, commit, sarif)
    ledger = save_ledger(token, entry)
    upload_reports(token, ledger, project)
    print(f'Uploaded SARIF/{project}/{filename}; generated project and consolidated Excel reports')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sarif', required=True, help='Path to SARIF 2.1.0 file')
    parser.add_argument('--project', required=True, help='OneDrive project folder name')
    args = parser.parse_args()
    run(args.sarif, args.project)


if __name__ == '__main__':
    try:
        main()
    except (OSError, HTTPError, URLError, ValueError, RuntimeError, json.JSONDecodeError) as error:
        print(f'SARIF report workflow failed: {error}', file=sys.stderr)
        sys.exit(1)
