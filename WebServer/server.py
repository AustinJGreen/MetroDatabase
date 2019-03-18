from flask import Flask, request, render_template
import MySQLdb
from dbconnect import connection

app = Flask(__name__)

@app.route('/')
def my_form():
    return render_template('my-form.html')

@app.route('/', methods=['POST'])
def my_form_post():
    text = request.form['text']
    processed_text = text.upper()
    return processed_text


#No register page currently.
@app.route('/register/', methods=["GET","POST"])
def register_page():
    try:
        c, conn = connection()
        return("okay")
    except Exception as e:
        return(str(e))
#


#def connection():
#    conn = MySQLdb.connect(host='metrodb.c6yfhb0actbf.us-west-2.rds.amazonaws.com',
#                           user = 'guest',
#                           passwd = 'guest',
#                           db = 'metrodb')
#    c = conn.cursor()
#
#    return c, conn


#GET, PUT, POST, DELETE, etc

#if request.method == 'POST':
#name=request.form['name']