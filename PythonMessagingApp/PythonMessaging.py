
import kivy
kivy.require('1.0.8')
from kivy.app import App  
from kivy.core.window import Window
from kivy.uix.textinput import TextInput
from kivy.uix.button import Button
from kivy.uix.label import Label
#from kivy.base import runTouchApp
from kivy.uix.boxlayout import BoxLayout 
from kivy.clock import Clock
import pyodbc
import threading
import random
import string

class PythonMessaging(App): 

    def on_button_press(self, instance):
        text_from_btn = self.ti_compose.text
        query_string = 'INSERT INTO dbo.ServerMessages (Message, SenderId, ReceiverId) VALUES(\'{0}\',\'{1}\',\'{2}\')'.format(self.ti_compose.text, "Python", "All")
        self.cursor.execute(query_string)
        self.cnxn.commit()
        self.ti_compose.text = ''

    def load_all_messages(self):
        self.ti_messages.text = ''
        query_string = 'SELECT * FROM dbo.ServerMessages'
        self.cursor.execute(query_string)
        row = self.cursor.fetchone()
        while row:
            self.ti_messages.text += row[3] + ': ' + row[1] + '\n'
            self.last_msg_id = row[0]
            row = self.cursor.fetchone()

    def load_new_messages(self, other):
        query_string = 'SELECT * FROM dbo.ServerMessages WHERE Id > {0}'.format(self.last_msg_id)
        self.cursor.execute(query_string)
        row = self.cursor.fetchone()
        while row:
            self.ti_messages.text += row[3] + ': ' + row[1] + '\n'
            self.last_msg_id = row[0]
            row = self.cursor.fetchone()

    def test(self, other):
        letters = string.ascii_lowercase
        random_string = ''.join(random.choice(letters) for i in range(random.randint(1,256)))
        self.ti_compose.text = random_string
        self.on_button_press(None)

    def on_test(self, instance):
        Clock.schedule_interval(self.test, 6)

    def build(self):
        b = BoxLayout(orientation ='vertical', )

        # Adding the text input  
        self.ti_messages = TextInput(font_size = 10,  size_hint_y = None,  height = 300, width = 200)  
        self.ti_compose = TextInput(font_size = 10,  size_hint_y = None,  height = 100, width = 200)  

        # Adding Button and styling 
        self.bt_send = Button(text ="Send", font_size ="10sp")
        self.bt_send.bind(on_press = self.on_button_press)

        self.bt_test = Button(text = "Test", font_size = "10sp")
        self.bt_test.bind(on_press = self.on_test)

        b.add_widget(self.ti_messages)  
        b.add_widget(self.ti_compose)  
        b.add_widget(self.bt_send) 
        b.add_widget(self.bt_test) 

        self.last_msg_id = 0
        self.CONNECTION_STRING = 'Driver={ODBC Driver 17 for SQL Server};SERVER=localhost\SQLEXPRESS;Database=MessagingDb;Trusted_Connection=yes;'
        self.cnxn = pyodbc.connect(self.CONNECTION_STRING)
        self.cursor = self.cnxn.cursor()
        self.load_all_messages()

        Clock.schedule_interval(self.load_new_messages, 5)
      
        return b 
    

  
# Run the App  
if __name__ == "__main__":  
    pm = PythonMessaging()
    pm.run()
