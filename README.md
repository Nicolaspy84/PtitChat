# ReadMe

Welcome to PtitChat, our peer-to-peer chat application to chat with your friends and have fun !

Summary:
1. Presentation
2. Installation and run\
    a) Run project with interface \
    b) Run project with terminal


## 1. Presentation

With the PtitChat application you can:
- Join a chat room, by connecting to a peer wich is already in
- Send and receive private messages
- Send and receive files 

PtitChat does not have a server, the messages are transmitted throught the peers in order to be readen by all users.

### Message and file transmission
------expliquer les rumeurs -----


## 2. Installation and run

### a) Run project with interface

To run the project with the interface, you need to use Windows.

1. In `PtitChat/Console/Interface/bin/Debug`, click on the Interface application. If you do not habe the bin forlder, run the project one time with Visual Studio.
2. Connect: enter the username you want to use and the port you want to use

#### Connect to a friend or a chat room

To connect to a friend, type your friends ip address and port under "Entrez une adresse ip pour vous connecter", with the form
```
ip_address:port_number
```
(ex: 182.100.100.100:3000)

By connecting to a friend, you will enter the chat room, you can see it by clicking on "Tous".\
In this chat room, you can see all the messages sended before your arrival by all the users connected directly or indirectly to you.


#### Send and receive private messages

Under "Tous", you see the usernames of the users connected to you, directly or through other peers.\
Click on one of them to access to your private conversation, you can send messages through it, wich will remain private.

`IMPORTANT:` To receive a private message from somebody, he needs to know you. You have to connect to him manually (by entering his ip address), send him a message or send a message on the chat room.


#### Send and receive files

Files can be received and sended in private conversations only.

To send a file:
- click on the name of the person to whom you want to send the file
- click on the "Envoyer un ficher" button, on the bottom right of the window
- choose the file you want to send and click on "ouvrir"
You just sent a file to a friend !

When your receive a file, it's saved in a folder "Downloads", in the project. If the folder doesn't exist yet, it will be created.




### b) Run project with terminal

In `PtitChat/Console/PtitChat/bin/debug`, run the following command

```
mono PtitChat.exe
```
You can also run the PtitChat project in Visual Studio

Then follow the instructions, enter your username (ex: michel) and your port (ex: 3000).
If you want to connect to peers, respond Y to the question and enter their addresses with the form
```
ip_address:port_number ip_address_2:port_number_2
```


Commands:
 - `/peers` : display the peers you are connected to
 - `/pm username message` : to send a private message to a specific user
 - `/sendfile username file_path` : to send a file to a specific user
 - `/users`: displays all the users you know, with the messages they sent. You know a user when you received at least one message of him, in the chat room or privatly.
 - just type your message if you want to send a message on the chat

To go faster, you can give all the informations in one only command:

```
mono PtitChat.exe -username <username> -port <port> -peers <peer addresses>
```


When you receive a message, the console displays:\
  `username(msgID) @<date> : message`


