// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

//using log4net;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleHttpServer
{

    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener Listener;
        private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        //private static readonly ILog log = LogManager.GetLogger(typeof(HttpServer));

        #region Public Methods

        public void Stop() {
            this.IsActive = false;
            this.Listener.Stop();
        }

        readonly IApp _APP = null;
        public HttpServer(int port, List<Route> routes, IApp app)
        {
            _APP = app;

            this.Port = port;
            this.Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                this.Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            try
            {
                this.Listener = new TcpListener(IPAddress.Any, this.Port);
                //this.Listener = new TcpListener(IPAddress.Any, 0);            
                this.Listener.Start();
                while (this.IsActive)
                {
                    TcpClient s = this.Listener.AcceptTcpClient();
                    Thread thread = new Thread(new ParameterizedThreadStart((obj) =>
                    {
                        IApp app = (IApp)obj;
                        this.Processor.HandleClient(s, app);
                    }));
                    thread.Start(_APP);
                    Thread.Sleep(1);
                }
            }
            catch { }
        }

        #endregion

    }
}



