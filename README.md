## Securing Network Communication in a Multi-factor Password Manager - KeePass-server

This prgram functions as a server for KeePass password manager. The primary goal of KeePass-server is to store data transferred from KeePass and to send data to KeePass when requested. 

## System Requirements

This program currently uses version 4.6 of the .Net framewok. It also has been tested to successfully work on verson 4.8 of the .Net framework. 

All Windows operating systems later than Windows XP should support KeePass, and this program is tested under Windows10.

It should also be able to run on Linux and MacOS system using Mono to enable .Net functionality. But it has not been tested in these system.

**Run this program:**

run KeePassServer in comp8755-2020s2-keepass-server/KeePassServer/bin/Release

**Recompile this program:** 

Currently, KeePass-server has been successfully complied and you can simply click to run **release version** of KeePassServer.

If you would like to recomplie the original code, you can follow the following steps:

1. Make sure that you recomplie the Release version by selecting "**Release**" in solution configuration
2. Delete all previously complied files in Release folder (it should locate in  comp8755-2020s2-keepass-server/Build/KeePassServer/Release)
3. Right click KeePassServer Project to **Clean** and **Rebuild** KeePassServer Project.

## How to Use

As this program it used interactively with KeePass, please look at the "How to Use" section in the README.md file under KeePass for instruction.

## Project Structure

- **Network Util**
  
  - ApiArray.cs
  
    This is a utility class used to combine, slice arraries and add headers
  
  - ApiFile.cs
  
    This is a file utility class. It is used to read files in KeePass-server and load to byte array in a particular format and write particular format byte arrays into file system (formats depends on message structure specified by protocols)
  
  - EncryptionScheme.cs
  
    This is an encryption utility class implementing salted AES encryption and decryption and Diffie-Hellman Key exchange.
  
- **MainForm.cs**

  This WinForm class is the home page of this program. It is used to initiate communication interact with KeePass

- **ClientAuthenticationForm.cs**

  This WinForm is used for client authentication. User has to enter PIN shown on KeePass in this window.