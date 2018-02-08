﻿/* The MIT License (MIT)
* 
* Copyright (c) 2015 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

// From: http://stackoverflow.com/questions/34478513/c-sharp-full-duplex-asynchronous-named-pipes-net
// See Eric Frazer's Q and self answer
//
// Based on Marc Clifton's CodeProject article: https://www.codeproject.com/Articles/1179195/Full-Duplex-Asynchronous-Read-Write-with-Named-Pip?msg=5480792#_comments
//


namespace PipeLib.Core
{
    /// <summary>
    /// Arguments for a pipe message
    /// </summary>
    public sealed class PipeEventArgs
    {
        /// <summary>The raw data from a byte[] reader message</summary>
        public byte[] Data { get; set; }

        /// <summary>Creates a new instance of <see cref="PipeEventArgs"/> with <see cref="byte"/>s as data</summary>
        /// <param name="data">The argument bytes</param>
        public PipeEventArgs(byte[] data) => Data = data;

        /// <summary>The length of the string or the byte array depending on type</summary>
        public int Length =>  Data.Length;
        /// <summary>Display the event args as a string</summary>
        /// <returns>The string for string-type args, a byte array with length declaration otherwise</returns>
        public override string ToString() => $"byte[{Length}]";
    }
}
