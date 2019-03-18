from flask import Flask, flash, request, render_template
import MySQLdb
from dbconnect import connection
from rform import ReusableForm
import os

app = Flask(__name__)
app.secret_key = '*J/2SvTcP$*tccx'
app.config['SESSION_TYPE'] = 'filesystem'

@app.route('/')
def my_form():
    form = ReusableForm(request.form)
    print form.errors
    if request.method == 'POST':
        name = request.form['name']
        password=request.form['password']
        email=request.form['email']
        print('Got form')

    if form.validate():
            flash('Thanks for nothing lol!')
    else:
            flash('err u suck')

    return render_template('hello.html', form=form)

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

if __name__ == '__main__':
    sess.init_app(app)

    app.debug = True
    app.run()
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
