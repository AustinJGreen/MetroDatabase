from metrodb import MetroDB

# Try connecting to MetroDB
db = MetroDB('guest', 'guest')
db.connect()
print("Connected!")
baseTable = db.execute_command("SELECT * FROM BASE")
print(baseTable.fetch_row(maxrows=0))
db.close()


