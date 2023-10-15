# BackupTransferProtocol

Backup Transfer Protocol is a very simple system, desgined to be expanded, it's main goal is to put files out of reach from Ransomware, it is a Client/Server system consisting of two programs, a Client and a Server program.

Ransomware exploits systems by gaining access to the file system modifying files everywhere it has access to, file systems, file shares, online connected drives etc.

This system is very simple in it's approach install a computer somewhere on your network put the server code on it, only open the one port that the server is listening to (defaults to 47440), no file shares, use a completely different username and password on that computer, don't join it to a domain or network, but assign it a static IP address on the local network.

Once installed the client can copy files using the command 

BTPC [IP ADDRESS] filename

What this does on the server side is create the same folder structure as the client, it copies the file over to the server, the key point in this process is it will never overwrite an existing file on the server, it renames the original if one exists with the extension being the date and time of the rename, the new file gets copied in place.

So even if you get attacked and your files are modified, the next copy will only copy an encrypted file onto the server leaving the previous ones alone.

This is a very basic system, there is a lot of room for improvement, but an old PC with a large hard drive sitting on the network could make a difference if the network is attacked.

Reducing the attack surface is really important, which is why there is no restore or listing functionality, you have bigger issues at the point of an attack than easy access to file, the fact you have the files is what is important.







