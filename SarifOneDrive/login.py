
"""Authenticate a Microsoft personal account with device-code flow."""

import json
import os
import sys
import time
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


AUTHORITY = "https://login.microsoftonline.com/consumers"
SCOPES = "offline_access Files.ReadWrite"
TOKEN_FILE = Path(__file__).resolve().with_name("initial_tokens.json")


def post_form(url, values):
    request = Request(
        url,
        data=urlencode(values).encode("utf-8"),
        headers={"Content-Type": "application/x-www-form-urlencoded"},
        method="POST",
    )
    try:
        with urlopen(request, timeout=30) as response:
            return json.loads(response.read().decode("utf-8")), response.status
    except HTTPError as error:
        response_body = error.read().decode("utf-8", errors="replace")
        try:
            result = json.loads(response_body)
        except json.JSONDecodeError:
            result = {}
        return result, error.code
    except URLError as error:
        raise RuntimeError("Microsoft authentication request failed") from error


def main():
    client_id = os.environ.get("MS_CLIENT_ID", "").strip()
    if not client_id:
        raise ValueError("Set the MS_CLIENT_ID environment variable first")

    device, status = post_form(
        f"{AUTHORITY}/oauth2/v2.0/devicecode",
        {"client_id": client_id, "scope": SCOPES},
    )
    if status < 200 or status >= 300:
        raise RuntimeError(
            "Device-code request failed: {}".format(device.get("error", "unknown error"))
        )

    print(device["message"])
    poll_interval = int(device.get("interval", 5))
    deadline = time.monotonic() + int(device["expires_in"])

    while time.monotonic() < deadline:
        time.sleep(poll_interval)
        result, status = post_form(
            f"{AUTHORITY}/oauth2/v2.0/token",
            {
                "grant_type": "urn:ietf:params:oauth:grant-type:device_code",
                "client_id": client_id,
                "device_code": device["device_code"],
            },
        )

        if 200 <= status < 300:
            with TOKEN_FILE.open("w", encoding="utf-8") as token_file:
                json.dump(result, token_file, indent=2)
                token_file.write("\n")
            print("Authentication successful. Tokens saved locally.")
            return

        error = result.get("error")
        if error == "authorization_pending":
            continue
        if error == "slow_down":
            poll_interval += 5
            continue
        if error == "authorization_declined":
            raise RuntimeError("Device-code authentication was declined")
        raise RuntimeError(
            "Device-code authentication failed: {}".format(error or "unknown error")
        )

    raise TimeoutError("Device code expired before authentication completed")


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, RuntimeError, TimeoutError) as error:
        print("Authentication failed: {}".format(error), file=sys.stderr)
        sys.exit(1)
