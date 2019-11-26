# PtitChat - Group 6

Welcome to PtitChat, our peer-to-peer chat application to chat with your friends and have fun !

`Table of contents :`
1. Introduction
2. How it works
3. Install and run\
    a) Run project with interface \
    b) Run project with terminal
4. Example
5. Credits


# 1. Introduction

The PtitChat application is a decentralized chat room system, where you can :
- Join a general chat room and chat with anyone on this network
- Send and receive private messages to/from anyone on your network
- Send and receive files to/from anyone on your network

PtitChat does not work with a server, it's a decentralized system. Just run the application and connect to anyone else to create your own peer-to-peer network and share messages!

# 2. How it works

## Users
This system is a decentralized chat system. It creates and handles any number of users. A `user` is defined by a unique username and represents a physical person, we therefore assume that people agree to use different usernames.

## Peers
Users are connected to each others by connection sockets, that we call `peers`. Together, peers can form multiple dynamic networks, which can have any shapes : new peers can directly connect to other peers (via a TCP socket), and can disconnect at any moment from its network, closing all of its connections.

## Rumors
Users can send messages to every other user on a general chat by sending `rumors`. When a user sends a rumor on the network, it will be broadcasted to every one of its connected peers. Then if those peers have not seen this rumor yet they will broadcast it again to every one of their peers, and so on. This makes sure every peer of this network will see the new rumor. Finally, if 2 disconnected networks reconnect (usually a new peer will establish a connection in between the 2 networks), they will share all their rumors, thus merging into a single network.

## Private messages
Users can also send `private messages` to other users. When new rumors are received by peers, they update their routing table : for every user of the network, they know which connected peer they have to communicate with in order to send a direct message to this user (because this peer itself has a routing table and knows who to contact next to transmit the message). Therefore, when a user sends a private message to another user, the message follows a single route, which is assumed to be the fastest one (so there's no broadcasting like in the rumors' case).

## File sharing
Users can also share `files` with other users. A user can decide to send any file to another user. When he does so, the file is divided into chunks of fixed byte size, which are then sent one by one to their destination, in the same way private messages are sent. Once the destination user has received all the chunks, he will reconstruct the file and save it in a downloads folder.

## How multiple networks work together
When you start an instance of the program, you essentially first create your own little network. In this network, you are alone (a single node). You can send rumors, but for now, you will be the only one to see them. Then once you decide to connect to another peer, you merge your network with this new network (by exchanging with this peer all rumors known so far and spreading the new ones on our respective networks).


# 3. Install and run

## a) Run the project with an interface

To run the project with the interface, you need to use Windows (you can run the project in console mode on OSX).

1. In `Project/Interface/bin/Debug`, click on the Interface.exe application. If you do not have the bin forlder or the application, you need to generate the project with Visual Studio (run the PtitChat.sln project and then generate the Interface project - you will need C# .NET Framework 4.7 for this).
2. When the application prompts you to connect, enter a unique username and the port you want to use for communications.

### How to connect to a friend or a chat room

To connect to a friend, type your friend's ip address and port under "Entrez une adresse ip pour vous connecter", with the form :
```
ip_address:port_number
```
For example (use 127.0.0.1 if you are trying this in local instead of localhost) :
```
182.100.100.100:3000
```

By connecting to a friend (ie another peer), you will enter the chat room. You can see it by clicking on "Tous" and sending messages.\
In this chat room, you can also see all the messages sent before your arrival by all users connected directly or indirectly to you.


### How to send and receive private messages

Under "Tous", you see usernames of other users, connected directly or indirectly to you.\
Click on one of them to access to your private conversation. You will be able to send direct private messages to them.

`IMPORTANT:` To receive a private message from someone, he needs to know about you. You have to be connected to the network, and send him a private message or send a rumor on the chat room.


### How to send and receive files

Files can be received and sent in private conversations only.\
To send a file:
- click on the name of the user to whom you want to send the file
- click on "Envoyer un ficher", on the bottom right of the window
- choose the file you want to send and click on "ouvrir"

You just sent a file to a friend ! \
When your receive a file, it's saved in the `downloads/` folder, next to the executable. If the folder doesn't exist yet, it will be created once your file has been reconstructed.\
You can see all the files you already received and reconstructed properly by clicking on "Fichiers" on the left part.

## b) Run the project with a console

With your terminal/console, go in `Project/PtitChat/bin/debug` and run the following command (add mono in front of the command if you are on OSX) :

```
PtitChat.exe
```

If you don't have the application or the folder, you need to build the executable first in Visual Studio. Open the `PtitChat.sln` project in Visual Studio and generate the PtitChat solution to have the console executable under `Project/PtitChat/bin/debug`.

Then follow the instructions, enter your username (for example alice) and the port you would like to use for communications (for example 3000).\
If you want to connect to peers, answer Y to the question and enter their addresses with the form :
```
ip_address_1:port_1 ip_address_2:port_2
```
`IMPORTANT:` Use 127.0.0.1 instead of localhost if you are testing the application locally.


Possible commands:
 - `/peers` : displays the peers you are connected to
 - `/pm username message` : sends a private message to a specific user
 - `/sendfile username file_path` : sends a file to a specific user
 - `/users`: displays all the users you know with all the messages they sent (note that you will only know other users once you've received messages or rumors from this user)
 - just type any message if you want to broadcast a rumor on the shared chat

To go faster, you can give enter information in a single command when you run the executable (add mono in front of the command if you are on OSX) :

```
PtitChat.exe -username <username> -port <port> -peers <peer_addresses>
```

Whenever you receive a message, the console should display:\
```
username(msgID) @<date> : message_content
```

# Example
If you would like to test a basic networking example with the console, you can follow these steps. Please make sure the console executable is correctly built under `Project/PtitChat/bin/debug`, if not you will need to generate the solution (please refer to the previous section for this).

We will create a network with three nodes, A, B and C, connected as A-B-C (so A is connected to B and B is connected to C, but A is not connected to C, so B will have to transmit messages for them). They will send rumors and private messages, and finally A will send a file to C.

First, open three consoles, go to the folder containing the console application (it should be under `Project/PtitChat/bin/debug`), and run the following commands (one per console, don't forget to add mono in front of the commands if you are on OSX) :

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

If everything worked, you should see a "Reconstructed file" notification in user C's console, and find a new downloads/ folder in the same directory as your exectubale, with the downloaded file in it !

# Credits
Developpers of this project :
* **Nicolas Buisson**
* **Pierre Zimmermann**
* **Anne-Laurène Harmel**
