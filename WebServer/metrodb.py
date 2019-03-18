import MySQLdb

class MetroDB:

    def __init__(self, user, password):
        self.host = 'metrodb.c6yfhb0actbf.us-west-2.rds.amazonaws.com'
        self.port = 1997
        self.user = user
        self.password = password

    def connect(self):
        self.conn = MySQLdb.connect(host=self.host,
                                    user=self.user,
                                    passwd=self.password,
                                    port=self.port,
                                    db='metrodb')
        self.cur = self.conn.cursor()

    def connected(self):
        return self.conn is not None

    def close(self):
        if not self.connected():
            return False
        self.conn.close()
        return True

    def execute_command(self, command):
        if not self.connected():
            return False

        self.conn.query(command)
        return self.conn.store_result()
