/* --------------------------------------------------------------------------
 * Copyrights
 *
 * Portions created by or assigned to Cursive Systems, Inc. are
 * Copyright (c) 2002-2008 Cursive Systems, Inc.  All Rights Reserved.  Contact
 * information for Cursive Systems, Inc. is available at
 * http://www.cursive.net/.
 *
 * License
 *
 * Jabber-Net is licensed under the LGPL.
 * See LICENSE.txt for details.
 * --------------------------------------------------------------------------*/
using System;
using System.Threading;
using System.Xml;

using bedrock.util;
using jabber;
using jabber.client;
using jabber.protocol.client;
using jabber.protocol.iq;
using jabber.connection;

namespace ConsoleClient
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [SVN(@"$Id$")]
    class Class1
    {
        [CommandLine("j", "user@host Jabber ID", true)]
        public string jid = null;

        [CommandLine("p", "Password", false)]
        public string pass = null;

        [CommandLine("n", "Network Host", false)]
        public string networkHost = null;

        [CommandLine("o", "Port", false)]
        public int port = 5222;

        [CommandLine("t", "TLS auto-start", false)]
        public bool TLS = true;

        [CommandLine("r", "Register user", false)]
        public bool register = false;

        [CommandLine("c", "Certificate file", false)]
        public string certificateFile = null;

        [CommandLine("w", "Certificate password", false)]
        public string certificatePass = "";

        [CommandLine("u", "Untrusted certificates OK", false)]
        public bool untrustedOK = false;

        [CommandLine("i", "Do not send initial presence", false)]
        public bool initialPresence = true;

        [CommandLine("b", "HTTP Binding (BOSH) URL", false)]
        public string boshURL = null;

        public Class1(string[] args)
        {
            JabberClient m_jc = new JabberClient();
            m_jc.OnReadText += new bedrock.TextHandler(jc_OnReadText);
            m_jc.OnWriteText += new bedrock.TextHandler(jc_OnWriteText);
            m_jc.OnError +=new bedrock.ExceptionHandler(jc_OnError);
            m_jc.OnStreamError += new jabber.protocol.ProtocolHandler(jc_OnStreamError);

            m_jc.AutoReconnect = 3f;

            m_jc.Resource = "StagePresence";
            m_jc.AutoPresence = false;//true;//HACK: initialPresence;

            // connect over a BOSH stuff
            m_jc.Server = "chat-test.vix.tv";
            m_jc.NetworkHost = "chat-test.vix.tv";
            m_jc.Port = 443;
            m_jc[Options.POLL_URL] = "https://chat.vix.tv/http-bind/";

            // Set server based on BOSH URL
            Uri uri = new Uri(m_jc[Options.POLL_URL].ToString());
            m_jc.Server = uri.Host;
            m_jc[Options.CONNECTION_TYPE] = ConnectionType.HTTP_Binding;
            m_jc[Options.ANONYMOUS] = true;
            m_jc[Options.AUTO_ROSTER] = false;

            CapsManager cm = new CapsManager();
            cm.Stream = m_jc;
            cm.Node = "http://vix.tv";

            Console.WriteLine("Connecting");
            m_jc.Connect();
            Console.WriteLine("Connected");


            //GetOpt go = new GetOpt(this);
            //try
            //{
            //    go.Process(args);
            //}
            //catch (ArgumentException)
            //{
            //    go.UsageExit();
            //}

            //if (untrustedOK)
            //    jc.OnInvalidCertificate += new System.Net.Security.RemoteCertificateValidationCallback(jc_OnInvalidCertificate);

            //JID j = new JID(jid);
            //jc.User = j.User;
            //jc.Server = j.Server;
            //jc.NetworkHost = networkHost;
            //jc.Port = port;
            //jc.Resource = "Jabber.Net Console Client";
            //jc.Password = pass;
            //jc.AutoStartTLS = TLS;
            //jc.AutoPresence = initialPresence;

            //if (certificateFile != null)
            //{
            //    jc.SetCertificateFile(certificateFile, certificatePass);
            //    Console.WriteLine(jc.LocalCertificate.ToString(true));
            //}

            //if (boshURL != null)
            //{
            //    jc[Options.POLL_URL] = boshURL;
            //    jc[Options.CONNECTION_TYPE] = ConnectionType.HTTP_Binding;
            //    Uri uri = new Uri(jc[Options.POLL_URL].ToString());
            //    jc.Server = uri.Host;                
            //    jc[Options.ANONYMOUS] = true;
            //    jc[Options.AUTO_ROSTER] = false;
            //}
            
            //if (register)
            //{
            //    jc.AutoLogin = false;
            //    jc.OnLoginRequired +=
            //        new bedrock.ObjectHandler(jc_OnLoginRequired);
            //    jc.OnRegisterInfo += new RegisterInfoHandler(this.jc_OnRegisterInfo);
            //    jc.OnRegistered += new IQHandler(jc_OnRegistered);
            //}

            //CapsManager cm = new CapsManager();
            //cm.Stream = jc;
            //cm.Node = "http://vix.tv";
            
            //Console.WriteLine("Connecting");
            //jc.Connect();
            //Console.WriteLine("Connected");

            //while (true)
            //{
            //    
            //}
            string line;
            while ((line = Console.ReadLine()) != null) {
                if (line == "/clear") {
                    // Hm.... I wonder if this works on windows.
                    Console.Write("\x1b[H\x1b[2J");
                    continue;
                }
                if ((line == "/q") || (line == "/quit")) {
                    m_jc.Close();
                    break;
                }
                if (line.Trim() == "") {
                    continue;
                }
                try {
                    if (line == "</stream:stream>") {
                        m_jc.Write(line);
                    }
                    else {
                        // TODO: deal with stanzas that span lines... keep
                        // parsing until we have a full "doc".
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(line);
                        XmlElement elem = doc.DocumentElement;
                        if (elem != null)
                            m_jc.Write(elem);
                    }
                }
                catch (XmlException ex) {
                    Console.WriteLine("Invalid XML: " + ex.Message);
                }
            }           
        }

        bool jc_OnInvalidCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Invalid certificate ({0}):\n{1}", sslPolicyErrors.ToString(), certificate.ToString(true));
            return true;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            new Class1(args);
        }

        private void jc_OnReadText(object sender, string txt)
        {
            if (txt != " ")
                Console.WriteLine("RECV: " + txt);
        }

        private void jc_OnWriteText(object sender, string txt)
        {
            if (txt != " ")
                Console.WriteLine("SENT: " + txt);
        }

        private void jc_OnError(object sender, Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
            Environment.Exit(1);
        }

        private void jc_OnStreamError(object sender, System.Xml.XmlElement rp)
        {
            Console.WriteLine("Stream ERROR: " + rp.OuterXml);
            Environment.Exit(1);
        }

        private void jc_OnLoginRequired(object sender)
        {
            Console.WriteLine("Registering");
            JabberClient jc = (JabberClient) sender;
            jc.Register(new JID(jc.User, jc.Server, null));
        }

        private void jc_OnRegistered(object sender,
                                     IQ iq)
        {
            JabberClient jc = (JabberClient) sender;
            if (iq.Type == IQType.result)
                jc.Login();
        }

        private bool jc_OnRegisterInfo(object sender, Register r)
        {
            return true;
        }
    }
}
