# ReadMe

Welcome to PtitChat, our peer-to-peer chat application to chat with your friends and have fun !

Summary:
1. Introduction
2. How it works
3. Install and run\
    a) Run project with interface \
    b) Run project with terminal
4. Example


## 1. Introduction

The PtitChat application is a decentralized chat room system, where you can :
- Join a general chat room and chat with anyone on this network
- Send and receive private messages to/from anyone on your network
- Send and receive files to/from anyone on your network

PtitChat does not work with a server, it's a decentralized system. Just run the application and connect to anyone else to create your own peer-to-peer network and share messages!

## 2. How it works

### Users
This system is a decentralized chat system. It creates and handles any number of users. A "user" is defined by a unique username and represents a physical person, we therefore assume that people agree to use different usernames.

### Peers
Users are connected to each others by connection sockets, that we call "peers". Together, peers can form multiple dynamic networks, which can have any shapes : new peers can directly connect to other peers (via a TCP socket), and can disconnect at any moment from its network, closing all of its connections.

### Rumors
Users can send messages to every other user on a general chat by sending "rumors". When a user sends a rumor on the network, it will be broadcasted to every one of its connected peers. Then if those peers have not seen this rumor yet they will broadcast it again to every one of their peers, and so on. This makes sure every peer of this network will see the new rumor. Finally, if 2 disconnected networks reconnect (usually a new peer will establish a connection in between the 2 networks), they will share all their rumors, thus merging into a single network.

### Private messages
Users can also send "private messages" to other users. When new rumors are received by peers, they update their routing table : for every user of the network, they know which connected peer they have to communicate with in order to send a direct message to this user (because this peer itself has a routing table and knows who to contact next to transmit the message). Therefore, when a user sends a private message to another user, the message follows a single route, which is assumed to be the fastest one (so there's no broadcasting like in the rumors' case).

### File sharing
Users can also share "files" with other users. A user can decide to send any file to another user. When he does so, the file is divided into chunks of fixed byte size, which are then sent one by one to their destination, in the same way private messages are sent. Once the destination user has received all the chunks, he will reconstruct the file and save it in a downloads folder.

### How multiple networks work together
When you start an instance of the program, you essentially first create your own little network. In this network, you are alone (a single node). You can send rumors, but for now, you will be the only one to see them. Then once you decide to connect to another peer, you merge your network with this new network (by exchanging with this peer all rumors known so far and spreading the new ones on our respective networks).


## 3. Install and run

### a) Run project with interface

To run the project with the interface, you need to use Windows.

1. In `PtitChat/Console/Interface/bin/Debug`, click on the Interface application. If you do not have the bin forlder, run the project one time with Visual Studio.
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

When your receive a file, it's saved in a folder "Downloads", in the project. If the folder doesn't exist yet, it will be created.\
You can see all the files you already received by clicking on "Fichiers" on the left part.





### b) Run project with terminal

With console, go in `PtitChat/Console/PtitChat/bin/debug` and run the following command

```
mono PtitChat.exe
```
You can also run the PtitChat project in Visual Studio

Then follow the instructions, enter your username (ex: michel) and your port (ex: 3000).\
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

## Example
If you would like to test a basic networking example with the console, you can follow these steps.

We will create a network with three nodes, A, B and C, connected as A-B-C (so A is connected to B and B is connected to C, but A is not connected to C, so B will have to transmit messages for them). They will send rumors and private messages, and finally A will send a file to C.

First, open three consoles, go to the folder containing the console application, and run the following commands (one per console, don't forget to add mono in front of the commands if you are on OSX) :

```
PtitChat.exe -username A -port 1001
```
For the first console, enter 'n' when asked if you would like to connect to other peers.
```
PtitChat.exe -username B -port 1002 -peers 127.0.0.1:1001
```
```
PtitChat.exe -username C -port 1003 -peers 127.0.0.1:1002
```

You can now send rumors in any console. Try it for every user, you should see the rumor appear in other consoles. To send a private message from user A to user C, write in user A's console :

```
/sendPM C Hello C, this is a private message!
```

You can also send files! To send a file to C, write this in A's console, where \<filepath> is the path of the file you'd like to share (for example test.txt if this file is in the same folder as your console executable) :

```
/sendfile C <filepath>
```

If everything worked, you should see a "Reconstructed file" notification in user C's console, and find a downloads/ folder in the same directory as your exectubale, with the doanloaded file in it !
