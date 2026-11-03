
#!/usr/bin/env python3
"""Upload original SARIF files to project-specific OneDrive folders."""

import argparse
import json
import os
import re
import subprocess
import sys

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


# --------------------------------------------------
# Main workflow
# --------------------------------------------------

def run(sarif_path, project_name):
    client_id = os.environ.get(
        "MS_CLIENT_ID",
        ""
    ).strip()

    refresh_token = os.environ.get(
        "ONEDRIVE_REFRESH_TOKEN",
        ""
    )

    repository = os.environ.get(
        "GITHUB_REPOSITORY",
        ""
    ).strip()

    commit = os.environ.get(
        "GITHUB_SHA",
        ""
    ).strip()

    if not client_id or not refresh_token or not repository or not commit:
        raise ValueError(
            "MS_CLIENT_ID, ONEDRIVE_REFRESH_TOKEN, "
            "GITHUB_REPOSITORY and GITHUB_SHA must be configured"
        )

    # Validate and preserve original SARIF bytes.
    sarif_bytes = read_sarif(sarif_path)

    # Use one consistent, safe folder name.
    safe_project_name = normalize_project_name(project_name)

    timestamp = os.environ.get(
        "SARIF_TIMESTAMP",
        ""
    ).strip()

    if not timestamp:
        timestamp = datetime.now(
            timezone.utc
        ).strftime("%Y%m%d-%H%M%S")

    filename = make_filename(
        safe_project_name,
        timestamp
    )

    # Authenticate.
    access_token, rotated_refresh_token = refresh_access_token(
        client_id,
        refresh_token
    )

    # Create SARIF/<ProjectName> if needed.
    ensure_sarif_folders(
        access_token,
        safe_project_name
    )

    # Upload without ZIP compression.
    upload_sarif(
        access_token,
        safe_project_name,
        filename,
        sarif_bytes
    )

    # Preserve the existing token rotation behavior.
    if (
        rotated_refresh_token
        and rotated_refresh_token != refresh_token
    ):
        update_github_refresh_secret(
            repository,
            rotated_refresh_token
        )

    print(
        "Uploaded SARIF/{}/{}".format(
            safe_project_name,
            filename
        )
    )


def main():
    parser = argparse.ArgumentParser(
        description=__doc__
    )

    parser.add_argument(
        "--sarif",
        required=True,
        help="Path to a SARIF 2.1.0 file"
    )

    parser.add_argument(
        "--project",
        required=True,
        help="Project name used as the OneDrive folder"
    )

    arguments = parser.parse_args()

    run(
        arguments.sarif,
        arguments.project
    )


if __name__ == "__main__":
    try:
        main()
    except (
        OSError,
        json.JSONDecodeError,
        ValueError,
        RuntimeError
    ) as error:
        print(
            "OneDrive SARIF upload failed: {}".format(error),
            file=sys.stderr
        )
        sys.exit(1)
