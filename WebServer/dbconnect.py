import MySQLdb
def connection():
    conn = MySQLdb.connect(host='metrodb.c6yfhb0actbf.us-west-2.rds.amazonaws.com',
                           user = 'guest',
                           passwd = 'guest',
                           db = 'metrodb')
    c = conn.cursor()

    return c, conn