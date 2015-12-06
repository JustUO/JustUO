using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Server.Accounting;
using Server;
using Server.Network;
using Server.Misc;

namespace Scripts.Engines.Encryption
{
    // This class handles OSI client encryption for clients newer than 2.0.3. (not including 2.0.3)
    public class Encryption : IPacketEncoder
    {
        // Encryption state information
        private uint m_Seed;
        private bool m_Seeded;
        private ByteQueue m_Buffer;
        private IClientEncryption m_Encryption;
        private bool m_AlreadyRelayed;

        public Encryption()
        {
            m_AlreadyRelayed = false;
            m_Encryption = null;
            m_Buffer = new ByteQueue();
            m_Seeded = false;
            m_Seed = 0;
        }

        static public void Initialize()
        {
            // Only initialize our subsystem if we're enabled
            if (Configuration.Enabled)
            {
                // Initialize static members and connect to the creation callback of a NetState.
                NetState.CreatedCallback = new NetStateCreatedCallback(NetStateCreated);

                // Overwrite the packet handler for the relay packet since we need to change the
                // encryption mode then.
                PacketHandlers.Register(0xA0, 3, false, new OnPacketReceive(HookedPlayServer));
            }
        }

        public static void NetStateCreated(NetState state)
        {
            state.PacketEncoder = new Encryption();
        }

        public static void HookedPlayServer(NetState state, PacketReader pvSrc)
        {
            // Call the original handler
            PacketHandlers.PlayServer(state, pvSrc);

            // Now indicate, that the state has been relayed already. If it's used again, 
            // it means we're entering a special encryption state
            Encryption context = (Encryption)(state.PacketEncoder);
            context.m_AlreadyRelayed = true;
        }

        // Try to encrypt outgoing data.
        public void EncodeOutgoingPacket(NetState to, ref byte[] buffer, ref int length)
        {
            if (m_Encryption != null)
            {
                m_Encryption.serverEncrypt(ref buffer, length);
                return;
            }
        }

        public void RejectNoEncryption(NetState ns)
        {
            // Log it on the console
            Console.WriteLine("Client: {0}: Unencrypted client detected, disconnected", ns);

            // Send the client the typical "Bad communication" packet and also a sysmessage stating the error
            ns.Send(new AsciiMessage(Server.Serial.MinusOne, -1, MessageType.Label, 0x35, 3, "System", "Unencrypted connections are not allowed on this server."));
            ns.Send(new AccountLoginRej(ALRReason.BadComm));

            // Disconnect the client
            ns.Dispose(true);
        }

        // Try to decrypt incoming data.
        public void DecodeIncomingPacket(NetState from, ref byte[] buffer, ref int length)
        {
            #region m_Encryption != null
            if (m_Encryption != null)
            {
                // If we're decrypting using LoginCrypt and we've already been relayed,
                // only decrypt a single packet using logincrypt and then disable it
                if (m_AlreadyRelayed && m_Encryption is LoginEncryption)
                {
                    uint newSeed = ((((LoginEncryption)(m_Encryption)).Key1 + 1) ^ ((LoginEncryption)(m_Encryption)).Key2);

                    // Swap the seed
                    newSeed = ((newSeed >> 24) & 0xFF) | ((newSeed >> 8) & 0xFF00) | ((newSeed << 8) & 0xFF0000) | ((newSeed << 24) & 0xFF000000);

                    // XOR it with the old seed
                    newSeed ^= m_Seed;

                    IClientEncryption newEncryption = new GameEncryption(newSeed);

                    // Game Encryption comes first
                    newEncryption.clientDecrypt(ref buffer, length);

                    // The login encryption is still used for this one packet
                    m_Encryption.clientDecrypt(ref buffer, length);

                    // Swap the encryption schemes
                    m_Encryption = newEncryption;
                    m_Seed = newSeed;

                    return;
                }

                m_Encryption.clientDecrypt(ref buffer, length);
                return;
            }
            #endregion

            #region Port Scan
            //11JUN2008 GOW SVN fix ** START ***
            // If the client did not connect on the game server port,
            // it's not our business to handle encryption for it
            //if (((IPEndPoint)from.Socket.LocalEndPoint).Port != Listener.Port) 
            //{
            //    m_Encryption = new NoEncryption();
            //    return;
            //}
            bool handle = false;

            for (int i = 0; i < Listener.EndPoints.Length; i++)
            {
                IPEndPoint ipep = (IPEndPoint)Listener.EndPoints[i];

                if (((IPEndPoint)from.Socket.LocalEndPoint).Port == ipep.Port)
                    handle = true;
            }

            if (!handle)
            {
                m_Encryption = new NoEncryption();
                return;
            }
            #endregion

            #region !m_Seeded
            // For simplicities sake, enqueue what we just received as long as we're not initialized
            m_Buffer.Enqueue(buffer, 0, length);
            // Clear the array
            length = 0;

            // If we didn't receive the seed yet, queue data until we can read the seed
            //if (!m_Seeded) 
            //{
            //    // Now check if we have at least 4 bytes to get the seed
            //    if (m_Buffer.Length >= 4) 
            //    {
            //        byte[] m_Peek = new byte[m_Buffer.Length];
            //        m_Buffer.Dequeue( m_Peek, 0, m_Buffer.Length ); // Dequeue everything
            //        m_Seed = (uint)((m_Peek[0] << 24) | (m_Peek[1] << 16) | (m_Peek[2] << 8) | m_Peek[3]);
            //        m_Seeded = true;

            //        Buffer.BlockCopy(m_Peek, 0, buffer, 0, 4);
            //        length = 4;
            //    } 
            //    else 
            //    {
            //        return;
            //    }
            //}
            //http://uodev.de/viewtopic.php?t=5097&postdays=0&postorder=asc&start=15&sid=dfb8e6c73b9e3eb95c1634ca3586e8a7
            //if (!m_Seeded)
            //{
            //    int seed_length = m_Buffer.GetSeedLength();

            //    if (m_Buffer.Length >= seed_length)
            //    {
            //        byte[] m_Peek = new byte[m_Buffer.Length];
            //        m_Buffer.Dequeue(m_Peek, 0, seed_length);

            //        if (seed_length == 4)
            //            m_Seed = (uint)((m_Peek[0] << 24) | (m_Peek[1] << 16) | (m_Peek[2] << 8) | m_Peek[3]);
            //        else if (seed_length == 21)
            //            m_Seed = (uint)((m_Peek[1] << 24) | (m_Peek[2] << 16) | (m_Peek[3] << 8) | m_Peek[4]);

            //        m_Seeded = true;

            //        Buffer.BlockCopy(m_Peek, 0, buffer, 0, seed_length);
            //        length = seed_length;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

            //11JUN2008 My Version

            if (!m_Seeded)
            {
                if (m_Buffer.Length <= 3) //Short Length, try again.
                {
                    Console.WriteLine("Encryption: Failed - Short Lenght");
                    return;
                }
                //else if ((m_Buffer.Length == 83) && (m_Buffer.GetPacketID() == 239)) //New Client
                //{
                //    byte[] m_Peek = new byte[21];
                //    m_Buffer.Dequeue(m_Peek, 0, 21);

                //    m_Seed = (uint)((m_Peek[1] << 24) | (m_Peek[2] << 16) | (m_Peek[3] << 8) | m_Peek[4]);
                //    m_Seeded = true;

                //    Buffer.BlockCopy(m_Peek, 0, buffer, 0, 21);
                //    length = 21;

                //    Console.WriteLine("Encryption: Passed - New Client");
                //}

                //05MAR2009 Smjert's fix for double log in.  *** START ***
                else if ((m_Buffer.Length == 83 || m_Buffer.Length == 21) && (m_Buffer.GetPacketID() == 239)) //New Client
                {
                    length = m_Buffer.Length;
                    byte[] m_Peek = new byte[21];
                    m_Buffer.Dequeue(m_Peek, 0, 21);

                    m_Seed = (uint)((m_Peek[1] << 24) | (m_Peek[2] << 16) | (m_Peek[3] << 8) | m_Peek[4]);
                    m_Seeded = true;

                    Buffer.BlockCopy(m_Peek, 0, buffer, 0, 21);


                    Console.WriteLine("Encryption: Passed - New Client");

                    // We need to wait the next packet
                    if (length == 21)
                        return;

                    length = 21;
                }

                else if (m_Buffer.Length >= 4) //Old Client
                //05MAR2009 Smjert's fix for double log in.  *** END ***
                {
                    byte[] m_Peek = new byte[4];
                    m_Buffer.Dequeue(m_Peek, 0, 4);

                    m_Seed = (uint)((m_Peek[0] << 24) | (m_Peek[1] << 16) | (m_Peek[2] << 8) | m_Peek[3]);
                    m_Seeded = true;

                    Buffer.BlockCopy(m_Peek, 0, buffer, 0, 4);
                    length = 4;

                    Console.WriteLine("Encryption: Passed - Old Client");
                }
                else //It should never reach here.
                {
                    Console.WriteLine("Encryption: Failed - It should never reach here");
                    return;
                }
            }
            #endregion

            // If the context isn't initialized yet, that means we haven't decided on an encryption method yet
            #region m_Encryption == null
            if (m_Encryption == null)
            {
                int packetLength = m_Buffer.Length;
                int packetOffset = length;
                m_Buffer.Dequeue(buffer, length, packetLength); // Dequeue everything
                length += packetLength;

                // This is special handling for the "special" UOG packet
                if (packetLength >= 3)
                {
                    if (buffer[packetOffset] == 0xf1 && buffer[packetOffset + 1] == ((packetLength >> 8) & 0xFF) && buffer[packetOffset + 2] == (packetLength & 0xFF))
                    {
                        m_Encryption = new NoEncryption();
                        return;
                    }
                }

                // Check if the current buffer contains a valid login packet (62 byte + 4 byte header)
                // Please note that the client sends these in two chunks. One 4 byte and one 62 byte.
                if (packetLength == 62)
                {
                    Console.WriteLine("Checking packetLength 62 == " + packetLength);
                    // Check certain indices in the array to see if the given data is unencrypted
                    if (buffer[packetOffset] == 0x80 && buffer[packetOffset + 30] == 0x00 && buffer[packetOffset + 60] == 0x00)
                    {
                        if (Configuration.AllowUnencryptedClients)
                        {
                            m_Encryption = new NoEncryption();
                        }
                        else
                        {
                            RejectNoEncryption(from);
                            from.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        LoginEncryption encryption = new LoginEncryption();
                        if (encryption.init(m_Seed, buffer, packetOffset, packetLength))
                        {
                            Console.WriteLine("Client: {0}: Encrypted client detected, using keys of client {1}", from, encryption.Name);
                            m_Encryption = encryption;
                            Console.WriteLine("Encryption: Check 1");
                            byte[] packet = new byte[packetLength];
                            Console.WriteLine("Encryption: Check 2");
                            Buffer.BlockCopy(buffer, packetOffset, packet, 0, packetLength);
                            Console.WriteLine("Encryption: Check 3");
                            encryption.clientDecrypt(ref packet, packet.Length);
                            Console.WriteLine("Encryption: Check 4");
                            Buffer.BlockCopy(packet, 0, buffer, packetOffset, packetLength);
                            Console.WriteLine("Encryption: Check 5");
                            //return; //Just throwing this in.
                        }
                        else
                        {
                            Console.WriteLine("Detected an unknown client.");
                        }
                    }
                }
                else if (packetLength == 65)
                {
                    Console.WriteLine("Checking packetLength 65 == " + packetLength);
                    // If its unencrypted, use the NoEncryption class
                    if (buffer[packetOffset] == '\x91' && buffer[packetOffset + 1] == ((m_Seed >> 24) & 0xFF) && buffer[packetOffset + 2] == ((m_Seed >> 16) & 0xFF) && buffer[packetOffset + 3] == ((m_Seed >> 8) & 0xFF) && buffer[packetOffset + 4] == (m_Seed & 0xFF))
                    {
                        if (Configuration.AllowUnencryptedClients)
                        {
                            m_Encryption = new NoEncryption();
                        }
                        else
                        {
                            RejectNoEncryption(from);
                            from.Dispose();
                        }
                    }
                    else
                    {
                        // If it's not an unencrypted packet, simply assume it's encrypted with the seed
                        m_Encryption = new GameEncryption(m_Seed);

                        byte[] packet = new byte[packetLength];
                        Buffer.BlockCopy(buffer, packetOffset, packet, 0, packetLength);
                        m_Encryption.clientDecrypt(ref packet, packet.Length);
                        Buffer.BlockCopy(packet, 0, buffer, packetOffset, packetLength);
                    }
                }

                // If it's still not initialized, copy the data back to the queue and wait for more
                if (m_Encryption == null)
                {
                    Console.WriteLine("Encryption: Check - Waiting");
                    m_Buffer.Enqueue(buffer, packetOffset, packetLength);
                    length -= packetLength;
                    return;
                }
            }
            #endregion
        }
    }
}
