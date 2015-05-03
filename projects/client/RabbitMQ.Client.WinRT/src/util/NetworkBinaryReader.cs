// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2014 GoPivotal, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//  The contents of this file are subject to the Mozilla Public License
//  Version 1.1 (the "License"); you may not use this file except in
//  compliance with the License. You may obtain a copy of the License
//  at http://www.mozilla.org/MPL/
//
//  Software distributed under the License is distributed on an "AS IS"
//  basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
//  the License for the specific language governing rights and
//  limitations under the License.
//
//  The Original Code is RabbitMQ.
//
//  The Initial Developer of the Original Code is GoPivotal, Inc.
//  Copyright (c) 2007-2014 GoPivotal, Inc.  All rights reserved.
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RabbitMQ.Util
{
    /// <summary>
    /// RabbitMQ.Util.NetworkBinaryReader implementation for Windows RT.
    /// </summary>
    public class NetworkBinaryReader
    {
        private DataReader m_input;

        // per-operation timeout
        private int m_timeout;

        /// <summary>
        /// Construct a NetworkBinaryReader over the given input stream.
        /// </summary>
        public NetworkBinaryReader(IInputStream input)
        {
            m_input = new DataReader(input);
            m_input.UnicodeEncoding = UnicodeEncoding.Utf8;
            m_input.ByteOrder = ByteOrder.BigEndian;
        }

        public NetworkBinaryReader(Stream input)
        {
            m_input = new DataReader(input.AsInputStream());

            m_input.UnicodeEncoding = UnicodeEncoding.Utf8;
            m_input.ByteOrder = ByteOrder.BigEndian;
        }

        public int Timeout
        {
            get { return m_timeout; }
            set { m_timeout = value; }
        }

        ///<summary>Helper method for constructing a temporary
        ///BinaryReader over a byte[].</summary>
        public static BinaryReader TemporaryBinaryReader(byte[] bytes)
        {
            return new BinaryReader(new MemoryStream(bytes));
        }

        async public Task<uint> LoadAsync(uint len)
        {
            var op = m_input.LoadAsync(len);
            var nRead = await op;
            if (nRead <= 0 || op.Status != AsyncStatus.Completed)
            {
                throw new EndOfStreamException();
            }
            else
            {
                return nRead;
            }
        }

        async public Task<byte[]> ReadBytesAsync(uint len)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(m_timeout);
            uint nRead;

            // TODO: this is not as efficient as it can be
            //       and largely matches how the non-WinRT NetworkBinaryReader
            //       works at the moment. Both should be moved to async/await
            //       eventually.
            var op = LoadAsync(len);
            if (op.Wait(m_timeout))
            {
                nRead = await op;
                if (nRead > 0)
                {
                    var bytes = new byte[len];
                    m_input.ReadBytes(bytes);

                    return bytes;
                }
                else
                {
                    throw new EndOfStreamException();
                }
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public byte[] ReadBytes(uint len)
        {
            Task<byte[]> future = ReadBytesAsync(len);
            if (future.IsFaulted || future.IsCanceled)
            {
                throw future.Exception;
            }
            else
            {
                return future.Result;
            }
        }

        public byte[] ReadBytes(int len)
        {
            return ReadBytes((uint)len);
        }

        public byte ReadByte()
        {
            var cts = new CancellationTokenSource(m_timeout);
            LoadAsync(1).Wait(cts.Token);
            return m_input.ReadByte();
        }

        public sbyte ReadSByte()
        {
            var cts = new CancellationTokenSource(m_timeout);
            LoadAsync(1).Wait(cts.Token);
            return (sbyte)m_input.ReadByte();
        }

        public double ReadDouble()
        {
            byte[] bytes = ReadBytes(8);
            byte temp = bytes[0];
            bytes[0] = bytes[7];
            bytes[7] = temp;
            temp = bytes[1];
            bytes[1] = bytes[6];
            bytes[6] = temp;
            temp = bytes[2];
            bytes[2] = bytes[5];
            bytes[5] = temp;
            temp = bytes[3];
            bytes[3] = bytes[4];
            bytes[4] = temp;
            return TemporaryBinaryReader(bytes).ReadDouble();
        }

        public short ReadInt16()
        {
            uint i = (uint)BitConverter.ToUInt16(ReadBytes(2), 0);
            return (short)(((i & 0xFF00) >> 8) |
                           ((i & 0x00FF) << 8));
        }

        public int ReadInt32()
        {
            uint i = (uint)BitConverter.ToUInt32(ReadBytes(4), 0);
            return (int)(((i & 0xFF000000) >> 24) |
                         ((i & 0x00FF0000) >> 8) |
                         ((i & 0x0000FF00) << 8) |
                         ((i & 0x000000FF) << 24));
        }

        public long ReadInt64()
        {
            ulong i = (ulong)BitConverter.ToUInt64(ReadBytes(8), 0);
            return (long)(((i & 0xFF00000000000000) >> 56) |
                          ((i & 0x00FF000000000000) >> 40) |
                          ((i & 0x0000FF0000000000) >> 24) |
                          ((i & 0x000000FF00000000) >> 8) |
                          ((i & 0x00000000FF000000) << 8) |
                          ((i & 0x0000000000FF0000) << 24) |
                          ((i & 0x000000000000FF00) << 40) |
                          ((i & 0x00000000000000FF) << 56));
        }

        public float ReadSingle()
        {
            byte[] bytes = ReadBytes(4);
            byte temp = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = temp;
            temp = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = temp;
            return TemporaryBinaryReader(bytes).ReadSingle();
        }

        public ushort ReadUInt16()
        {
            uint i = (uint)BitConverter.ToUInt16(ReadBytes(2), 0);
            return (ushort)(((i & 0xFF00) >> 8) |
                            ((i & 0x00FF) << 8));
        }

        public uint ReadUInt32()
        {
            uint i = (uint)BitConverter.ToUInt16(ReadBytes(4), 0);
            return (((i & 0xFF000000) >> 24) |
                    ((i & 0x00FF0000) >> 8) |
                    ((i & 0x0000FF00) << 8) |
                    ((i & 0x000000FF) << 24));
        }

        public ulong ReadUInt64()
        {
            ulong i = (ulong)BitConverter.ToUInt64(ReadBytes(8), 0);
            return (((i & 0xFF00000000000000) >> 56) |
                    ((i & 0x00FF000000000000) >> 40) |
                    ((i & 0x0000FF0000000000) >> 24) |
                    ((i & 0x000000FF00000000) >> 8) |
                    ((i & 0x00000000FF000000) << 8) |
                    ((i & 0x0000000000FF0000) << 24) |
                    ((i & 0x000000000000FF00) << 40) |
                    ((i & 0x00000000000000FF) << 56));
        }
    }
}