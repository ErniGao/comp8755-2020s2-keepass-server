using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Security.Cryptography;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using KeePassServer.Network_Util;
using System.Net.Configuration;

//=======================================================================================
//
//        filename : MainForm.cs
//        description : this is the home page for this program. 
//                      It is used to initiate communication and interact with KeePass
//        created by Erni Gao at  Nov 2020
//   
//=======================================================================================

namespace KeePassServer
{
    public partial class MainForm : Form
    {
        private const int LISTENLIST = 5;  //maximum no. of clients waiting to connect to this server
        private const int MAXLEN = 1024 * 1024 * 3;    //max length of data server can receive from client at a time

        //message header
        private const byte CIDH = 0;   //client id
        private const byte CKEYH = 1;  //client public key
        private const byte SKEYH = 2;  //server public key
        private const byte SHAREH = 3;  //one share of KeePass password database
        private const byte REQUESTH = 4;   //request share list
        private const byte LISTH = 5;   //list of all shares held in server
        private const byte SELECTEDSHAREH = 6;  //select one share stored in server
        private const byte RETRIEVEH = 61;  //retrieve a selected share
        private const byte DELETEH = 62;    //delete a selected share


        //step header 
        private const byte STEP0H = 0;
        private const byte STEP1H = 1;
        private const byte STEP2H = 2;
        private const byte STEP3H = 3;
        private const byte STEP4H = 4;
        private const byte STEP5H = 5;
        private const byte STEP6H = 6;

        private Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();    //client's ip address and its associated socket

        private string clientID;    //indicates current client this server transport data to 
        private byte[] commonKey;   //derived common key to encrypt & decrypt data
        
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cancel the check for cross thread calls when loading form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// start listening potential clients when click "Listen" button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnListen_Click(object sender, EventArgs e)
        {
            //get server's ip adddress and bind its port number
            IPAddress ip = IPAddress.Any;
            IPEndPoint endPoint = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));

            //create a socket for listening
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //associate network address to this listening socket
                listener.Bind(endPoint);

                //set the max number of clients that can connect to server at a time
                listener.Listen(LISTENLIST);
                showMessage("Server starts listening");

                //use another thread to listen incoming clients
                Thread th1 = new Thread(Listen);
                th1.IsBackground = true;
                th1.Start(listener);
            }
            catch
            {
            }
        }

        /// <summary>
        /// KeePass-server listens to all incoming clients
        /// Listen socket will create a new socket to send message when it accepts a client's connection
        /// </summary>
        /// <param name="o"></param>
        private void Listen(object o)
        {
            Socket listener = o as Socket;
            while (true)
            {
                try
                {
                    //create a socket used to send message when KeePass-server connects with a new client
                    Socket socketSend = listener.Accept();

                    //use another thread to receive message from client
                    Thread th2 = new Thread(Receive);
                    th2.IsBackground = true;
                    th2.Start(socketSend);

                    System.Threading.Thread.Sleep(1000);

                    while (true)
                    {
                        if (clientID != null)
                        {
                            //add the socket that is used to send message to comboBox
                            string socketID = socketSend.RemoteEndPoint.ToString() + " " + clientID;
                            comboClient.Items.Add(socketID);

                            //pair client id with client socket
                            dicSocket.Add(socketID, socketSend);

                            showMessage(socketID + ": connect successfully");
                            showBoldMessage("Please use \"Client List\" to select a client");
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// receive data sent from clients
        /// </summary>
        /// <param name="o">socket used to send message</param>
        private void Receive(object o)
        {
            Socket socketSend = o as Socket;

            while (true)
            {
                try
                {
                    // receive data from KeePass and store it in buffer
                    byte[] buffer = new byte[MAXLEN];
                    int len = socketSend.Receive(buffer);

                    if (len == 0)
                    { //break this loop when KeePass-server cannot receive any data from KeePass
                        break;
                    }

                    //seperate headers from data received
                    byte msgHeader = buffer[0];
                    byte stepHeader = buffer[1];
                    byte[] data = new byte[len - 2];
                    Array.Copy(buffer, 2, data, 0, len - 2);

                    if (msgHeader == CIDH && stepHeader == STEP0H)
                    {   //get client ID to see which client that KeePass-server communicates to

                        clientID = System.Text.Encoding.UTF8.GetString(data);
                    }
                    else if(msgHeader == CKEYH && stepHeader == STEP1H) 
                    {   //receive KeePass' public key and send KeePass-server's public key to KeePass

                        //use hashed public key for KeePass authentication
                        byte[] hashed_CK = new MD5CryptoServiceProvider().ComputeHash(data);
                        ClientAuthenticationForm caForm = new ClientAuthenticationForm(hashed_CK);
                        caForm.ShowDialog();

                        //use Diffie Hellman to generate KeePass-server's public key
                        ECDiffieHellmanCng ecd;
                        byte[] serverPublicKey;
                        EncryptionScheme.publicKeyGenerator(out ecd, out serverPublicKey);

                        //use hashed public key for server authentication
                        byte[] hashed_SK = new MD5CryptoServiceProvider().ComputeHash(serverPublicKey);
                        txtLog.Clear();
                        showRedMessage("Please enter " + Convert.ToBase64String(hashed_SK).Substring(0, 6) + " in client for authentication");

                        //use KeePass and server's public key to derive common key
                        commonKey = EncryptionScheme.deriveCommonKey(ecd, data);
                        
                        //send KeePass-server's public key to KeePass
                        byte[] sKeyBuffer = ApiArray.addHeader(SKEYH, STEP2H, serverPublicKey);
                        socketSend.Send(sKeyBuffer);
                    }
                    else if (stepHeader == STEP3H)
                    {
                        if (msgHeader == SHAREH)
                        {   //save shares sent from KeePass

                            // decrypt data received from KeePass
                            byte[] decryptedContent = EncryptionScheme.saltedDecryption(data, commonKey);
                        
                            //save the decrpted share in KeePass-server
                            ApiFile.writeAllFiles(clientID, decryptedContent);

                            //empty common key when transfer session ends
                            commonKey = null;
                            showMessage("Share sent from KeePass has been saved successfully");

                        }
                        else if (msgHeader == REQUESTH)
                        {   //send names of all shares available on KeePass-server to KeePass
                         
                            // get the path that stored shares for a particular client
                            DirectoryInfo[] clientDirs = ApiFile.listContents(Directory.GetCurrentDirectory());
                            string clientPath = "";
                            for (int i = 0; i < clientDirs.Length; i++)
                            {
                                if (clientDirs[i].Name == clientID)
                                {
                                    clientPath = clientDirs[i].FullName;
                                }
                            }

                            //KeePass-server loads all folder names that are stored for a particular client
                            DirectoryInfo[] clientFiles = ApiFile.listContents(clientPath);
                            string names = "";
                            for (int i = 0; i < clientFiles.Length; i++)
                            {
                                names += clientFiles[i];
                                if (i < clientFiles.Length - 1)
                                {
                                    names += "-";
                                }
                            }
                            showMessage("All shares held by server are: " + names);

                            //encrypt these folder names using common key
                            byte[] shareNames = EncryptionScheme.saltedEncryption(System.Text.Encoding.UTF8.GetBytes(names), commonKey);
                            shareNames = ApiArray.addHeader(LISTH, STEP4H, shareNames);
                            socketSend.Send(shareNames);
                        }   
                    }
                    else if (msgHeader == SELECTEDSHAREH && stepHeader == STEP5H)
                    {   
                        //get command message
                        byte commandMsg = data[0];

                        //get encrypted data
                        byte[] content = new byte[data.Length - 1];
                        Array.Copy(data, 1, content, 0, content.Length);

                        // decrypt data
                        byte[] shareName = EncryptionScheme.saltedDecryption(content, commonKey);

                        if (commandMsg == RETRIEVEH)
                        {
                            //retrieve all files under this path
                            string path = ApiFile.getDir(clientID, System.Text.Encoding.UTF8.GetString(shareName));
                            byte[] shareBuffer = ApiFile.readAllFiles(path);

                            //encrypt data
                            shareBuffer = EncryptionScheme.saltedEncryption(shareBuffer, commonKey);

                            //send data to KeePass
                            shareBuffer = ApiArray.addHeader(SHAREH, STEP6H, shareBuffer);
                            socketSend.Send(shareBuffer);
                            showMessage("Server has successfully sent the encrypted " + System.Text.Encoding.UTF8.GetString(shareName) + " to client.");

                            //finish the retrieval session and empty the common key
                            commonKey = null;

                        }
                        else if (commandMsg == DELETEH)
                        {
                            //delete a particular share stored in KeePass-server
                            string shareNameString = System.Text.Encoding.UTF8.GetString(shareName);
                            string path = Path.Combine(System.Environment.CurrentDirectory, clientID, shareNameString);
                            Directory.Delete(path, true);
                            showMessage(shareNameString + " has been deleted successfully");

                            //finish the delect session and empty the common key
                            commonKey = null;
                        }
                    }
                }

                catch
                {

                }
            }
        }

        /// <summary>
        /// show messages on text box
        /// </summary>
        /// <param name="str">messages that will be shown on message log</param>
        void showMessage(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        /// <summary>
        /// show message on text box in bold font
        /// </summary>
        /// <param name="str">messages that will be shown on message log</param>
        void showBoldMessage(string str)
        {
            int index = txtLog.Text.Length;
            showMessage(str);
            txtLog.Select(index, str.Length);
            txtLog.SelectionColor = Color.Black;
            Font font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Bold);
            txtLog.SelectionFont = font;
        }

        /// <summary>
        /// show message in red on text box
        /// </summary>
        /// <param name="str">messages that will be shown on message log</param>
        void showRedMessage(string str)
        {
            int index = txtLog.Text.Length;
            showMessage(str);
            txtLog.Select(index, str.Length);
            txtLog.SelectionColor = Color.Red;
            Font font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Bold);
            txtLog.SelectionFont = font;
        }
    }
}
