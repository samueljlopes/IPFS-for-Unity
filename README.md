# IPFS-for-Unity
An IPFS implementation for Unity. Includes asset storage in a linked-link format.

InterPlanetary File System (IPFS) is a protocol and network designed to create a content-addressable, peer-to-peer method of storing and sharing hypermedia in a distributed file system.

This a Unity Engine Implementation.

I haven’t tested it outside the Editor yet, but the example scene included contains 4 objects (3 may be deactivated on opening the scene, just make them active again.) 
 
The scripts on the objects, named ‘IPFS Object’ names the object visible to the Genesis script. The IPFS Genesis script contains one public variable ‘Write’. If true, it writes to the IPFS. If false, it reads from the IPFS the LAST THING YOU SAVED. 

You should also have a plugins folder with lost of .dlls. Please don't mess with them. They were a pain to make and get working.
