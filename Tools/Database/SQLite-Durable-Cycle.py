#!/usr/bin/env python3
import sqlite3
import sys
from pathlib import Path

KEY = "first-record"
VALUE = b"survives-restart"


def connect(path: Path) -> sqlite3.Connection:
    connection = sqlite3.connect(path, timeout=5.0, isolation_level=None)
    connection.execute("PRAGMA busy_timeout = 5000")
    connection.execute("PRAGMA journal_mode = DELETE")
    connection.execute("PRAGMA synchronous = FULL")
    return connection


def main() -> int:
    if len(sys.argv) == 2 and sys.argv[1] == "version":
        print(sqlite3.sqlite_version)
        return 0
    if len(sys.argv) != 3 or sys.argv[1] not in {"initialize", "put", "get"}:
        print(
            "usage: SQLite-Durable-Cycle.py version | <initialize|put|get> <database>",
            file=sys.stderr,
        )
        return 64
    operation = sys.argv[1]
    path = Path(sys.argv[2]).resolve()
    if operation == "initialize":
        with connect(path) as connection:
            connection.execute(
                "CREATE TABLE Records (Identity TEXT PRIMARY KEY, Payload BLOB NOT NULL)"
            )
        return 0
    if not path.is_file():
        return 2
    if operation == "put":
        with connect(path) as connection:
            connection.execute("BEGIN IMMEDIATE")
            connection.execute(
                "INSERT INTO Records (Identity, Payload) VALUES (?, ?)",
                (KEY, VALUE),
            )
            connection.execute("COMMIT")
        return 0
    with connect(path) as connection:
        row = connection.execute(
            "SELECT Payload FROM Records WHERE Identity = ?", (KEY,)
        ).fetchone()
    return 0 if row is not None and row[0] == VALUE else 3


if __name__ == "__main__":
    raise SystemExit(main())
