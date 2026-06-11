import sqlite3
import json

try:
    db = sqlite3.connect(r'e:\Project with unity\Tetris\.codegraph\codegraph.db')
    cursor = db.cursor()
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
    tables = cursor.fetchall()
    print("Tables:", tables)

    for table in tables:
        table_name = table[0]
        print(f"\n--- {table_name} ---")
        cursor.execute(f"PRAGMA table_info({table_name});")
        print("Columns:", cursor.fetchall())
        cursor.execute(f"SELECT * FROM {table_name} LIMIT 50;")
        rows = cursor.fetchall()
        for row in rows:
            print(row)
except Exception as e:
    print(f"Error: {e}")
