#!/usr/bin/env python3
"""Upload original SARIF to OneDrive and rotate the GitHub refresh-token secret."""

import argparse
from io import BytesIO
import json
import os
import re
import subprocess
import sys
from datetime import datetime, timezone
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, quote
from urllib.request import Request, urlopen
from zipfile import ZIP_DEFLATED, ZipFile


AUTHORITY = "https://login.microsoftonline.com/consumers"
TOKEN_ENDPOINT = AUTHORITY + "/oauth2/v2.0/token"
GRAPH_API = "https://graph.microsoft.com/v1.0"
SARIF_FOLDER = "SARIF"
GRAPH_SCOPE = "Files.ReadWrite offline_access"
SECRET_NAME = "ONEDRIVE_REFRESH_TOKEN"


def read_sarif(sarif_path):
    with open(sarif_path, "rb") as sarif_file:
        sarif_bytes = sarif_file.read()

    try:
        sarif = json.loads(sarif_bytes.decode("utf-8-sig"))
    except (UnicodeDecodeError, json.JSONDecodeError) as error:
        raise ValueError("Input must be a valid UTF-8 SARIF 2.1.0 document") from error

    if not isinstance(sarif, dict) or sarif.get("version") != "2.1.0":
        raise ValueError("Input must be a SARIF 2.1.0 document")
    if not isinstance(sarif.get("runs"), list):
        raise ValueError("SARIF document must contain a runs array")
    return sarif_bytes


def make_filename(repository, date_string):
    safe_repository = re.sub(r"[^A-Za-z0-9._-]+", "-", repository.replace("/", "-"))
    safe_repository = safe_repository.strip(".-_")
    if not safe_repository:
        raise ValueError("GITHUB_REPOSITORY does not contain a valid filename")
    return "{}_{}.zip".format(safe_repository, date_string)


def make_archive(repository, date_string, sarif_bytes):
    base_name = make_filename(repository, date_string)[:-4]
    archive_buffer = BytesIO()
    with ZipFile(archive_buffer, mode="w", compression=ZIP_DEFLATED) as archive:
        archive.writestr(base_name + ".sarif", sarif_bytes)
    return archive_buffer.getvalue()


def post_form(url, form):
    request = Request(
        url,
        data=urlencode(form).encode("utf-8"),
        headers={"Content-Type": "application/x-www-form-urlencoded"},
        method="POST",
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
                "scope": GRAPH_SCOPE,
            },
        )
    except HTTPError as error:
        raise RuntimeError(
            "Microsoft refresh-token request failed with HTTP {}".format(error.code)
        ) from error
    except URLError as error:
        raise RuntimeError("Microsoft refresh-token request failed") from error
    except (json.JSONDecodeError, UnicodeDecodeError) as error:
        raise RuntimeError("Microsoft returned an invalid token response") from error

    access_token = token_response.get("access_token")
    if not isinstance(access_token, str) or not access_token:
        raise RuntimeError("Microsoft token response did not include an access token")
    rotated_refresh_token = token_response.get("refresh_token")
    if rotated_refresh_token is not None and (
        not isinstance(rotated_refresh_token, str) or not rotated_refresh_token
    ):
        raise RuntimeError("Microsoft returned an invalid rotated refresh token")
    return access_token, rotated_refresh_token


def graph_request(
    access_token,
    url,
    method="GET",
    body=None,
    content_type="application/json",
):
    request_data = None
    if body is not None:
        request_data = json.dumps(body).encode("utf-8")
    request = Request(
        url,
        data=request_data,
        headers={
            "Authorization": "Bearer " + access_token,
            "Content-Type": content_type,
        },
        method=method,
    )
    try:
        with urlopen(request, timeout=30) as response:
            response_body = response.read()
            if not response_body:
                return response.status, {}
            try:
                return response.status, json.loads(response_body.decode("utf-8"))
            except (json.JSONDecodeError, UnicodeDecodeError) as error:
                raise RuntimeError("Microsoft Graph returned an invalid response") from error
    except HTTPError as error:
        raise error
    except URLError as error:
        raise RuntimeError("Microsoft Graph request failed") from error


def ensure_sarif_folder(access_token):
    folder_url = GRAPH_API + "/me/drive/root:/" + quote(SARIF_FOLDER, safe="")
    try:
        _, folder = graph_request(access_token, folder_url)
    except HTTPError as error:
        if error.code != 404:
            raise RuntimeError(
                "Could not check the OneDrive SARIF folder (HTTP {})".format(error.code)
            ) from error

        create_url = GRAPH_API + "/me/drive/root/children"
        try:
            _, folder = graph_request(
                access_token,
                create_url,
                method="POST",
                body={
                    "name": SARIF_FOLDER,
                    "folder": {},
                    "@microsoft.graph.conflictBehavior": "fail",
                },
            )
        except HTTPError as create_error:
            if create_error.code != 409:
                raise RuntimeError(
                    "Could not create the OneDrive SARIF folder (HTTP {})".format(
                        create_error.code
                    )
                ) from create_error
            try:
                _, folder = graph_request(access_token, folder_url)
            except HTTPError as lookup_error:
                raise RuntimeError(
                    "OneDrive SARIF folder creation conflicted and the folder "
                    "could not be opened"
                ) from lookup_error

    if not isinstance(folder, dict) or not isinstance(folder.get("folder"), dict):
        raise RuntimeError(
            "A non-folder item named {} already exists in OneDrive".format(SARIF_FOLDER)
        )


def upload_archive(access_token, filename, archive_bytes):
    encoded_filename = quote(filename, safe="._-")
    upload_url = (
        GRAPH_API
        + "/me/drive/root:/"
        + quote(SARIF_FOLDER, safe="")
        + "/"
        + encoded_filename
        + ":/content"
    )
    request = Request(
        upload_url,
        data=archive_bytes,
        headers={
            "Authorization": "Bearer " + access_token,
            "Content-Type": "application/zip",
        },
        method="PUT",
    )
    try:
        with urlopen(request, timeout=60) as response:
            if response.status < 200 or response.status >= 300:
                raise RuntimeError(
                    "OneDrive upload failed with HTTP {}".format(response.status)
                )
    except HTTPError as error:
        raise RuntimeError(
            "OneDrive upload failed with HTTP {}".format(error.code)
        ) from error
    except URLError as error:
        raise RuntimeError("OneDrive upload request failed") from error


def update_github_refresh_secret(repository, refresh_token):
    gh_token = os.environ.get("GH_SECRETS_PAT", "").strip()
    if not gh_token:
        raise ValueError(
            "GH_SECRETS_PAT is required to persist the rotated refresh token"
        )

    environment = os.environ.copy()
    environment["GH_TOKEN"] = gh_token
    try:
        result = subprocess.run(
            ["gh", "secret", "set", SECRET_NAME, "--repo", repository],
            input=refresh_token,
            text=True,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            check=False,
            env=environment,
            timeout=60,
        )
    except (OSError, subprocess.TimeoutExpired) as error:
        raise RuntimeError("Could not update the GitHub refresh-token secret") from error
    if result.returncode != 0:
        raise RuntimeError(
            "Could not update the GitHub refresh-token secret; verify the PAT permissions"
        )


def run(sarif_path):
    client_id = os.environ.get("MS_CLIENT_ID", "").strip()
    refresh_token = os.environ.get("ONEDRIVE_REFRESH_TOKEN", "")
    repository = os.environ.get("GITHUB_REPOSITORY", "").strip()
    commit = os.environ.get("GITHUB_SHA", "").strip()
    if not client_id or not refresh_token or not repository or not commit:
        raise ValueError(
            "MS_CLIENT_ID, ONEDRIVE_REFRESH_TOKEN, GITHUB_REPOSITORY, "
            "and GITHUB_SHA must be configured"
        )

    sarif_bytes = read_sarif(sarif_path)

    now = datetime.now(timezone.utc)
    filename = make_filename(repository, now.strftime("%Y-%m-%d"))
    archive_bytes = make_archive(repository, now.strftime("%Y-%m-%d"), sarif_bytes)

    access_token, rotated_refresh_token = refresh_access_token(client_id, refresh_token)
    ensure_sarif_folder(access_token)
    upload_archive(access_token, filename, archive_bytes)

    if rotated_refresh_token and rotated_refresh_token != refresh_token:
        update_github_refresh_secret(repository, rotated_refresh_token)

    print("Uploaded {}".format(filename))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sarif", required=True, help="Path to SARIF 2.1.0 input")
    arguments = parser.parse_args()
    run(arguments.sarif)


if __name__ == "__main__":
    try:
        main()
    except (OSError, json.JSONDecodeError, ValueError, RuntimeError) as error:
        print("OneDrive SARIF upload failed: {}".format(error), file=sys.stderr)
        sys.exit(1)
