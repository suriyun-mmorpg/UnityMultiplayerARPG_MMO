//+-------------------------------------------------------------------------------+
//| Copyright (c) 2003 Liping Dai. All rights reserved.                           |
//| Web: www.lipingshare.com                                                      |
//| Email: lipingshare@yahoo.com                                                  |
//|                                                                               |
//| Copyright and Permission Details:                                             |
//| =================================                                             |
//| Permission is hereby granted, free of charge, to any person obtaining a copy  |
//| of this software and associated documentation files (the "Software"), to deal |
//| in the Software without restriction, including without limitation the rights  |
//| to use, copy, modify, merge, publish, distribute, and/or sell copies of the   |
//| Software, subject to the following conditions:                                |
//|                                                                               |
//| 1. Redistributions of source code must retain the above copyright notice, this|
//| list of conditions and the following disclaimer.                              |
//|                                                                               |
//| 2. Redistributions in binary form must reproduce the above copyright notice,  |
//| this list of conditions and the following disclaimer in the documentation     |
//| and/or other materials provided with the distribution.                        |
//|                                                                               |
//| THE SOFTWARE PRODUCT IS PROVIDED �AS IS� WITHOUT WARRANTY OF ANY KIND,        |
//| EITHER EXPRESS OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED         |
//| WARRANTIES OF TITLE, NON-INFRINGEMENT, MERCHANTABILITY AND FITNESS FOR        |
//| A PARTICULAR PURPOSE.                                                         |
//+-------------------------------------------------------------------------------+

using System.IO;

namespace MultiplayerARPG.MMO
{
    namespace LipingShare.LCLib.Asn1Processor
    {
        /// <summary>
        /// IAsn1Node interface.
        /// </summary>
        internal interface IAsn1Node
        {
            /// <summary>
            /// Load data from Stream.
            /// </summary>
            /// <param name="xdata"></param>
            /// <returns>true:Succeed; false:failed.</returns>
            bool LoadData(Stream xdata);

            /// <summary>
            /// Save node data into Stream.
            /// </summary>
            /// <param name="xdata">Stream.</param>
            /// <returns>true:Succeed; false:failed.</returns>
            bool SaveData(Stream xdata);

            /// <summary>
            /// Get parent node.
            /// </summary>
            Asn1Node ParentNode { get; }

            /// <summary>
            /// Add child node at the end of children list.
            /// </summary>
            /// <param name="xdata">Asn1Node</param>
            void AddChild(Asn1Node xdata);

            /// <summary>
            /// Insert a node in the children list before the pointed index.
            /// </summary>
            /// <param name="xdata">Asn1Node</param>
            /// <param name="index">0 based index.</param>
            int InsertChild(Asn1Node xdata, int index);

            /// <summary>
            /// Insert a node in the children list before the pointed node.
            /// </summary>
            /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
            /// <param name="indexNode">Index node.</param>
            /// <returns>New node index.</returns>
            int InsertChild(Asn1Node xdata, Asn1Node indexNode);

            /// <summary>
            /// Insert a node in the children list after the pointed index.
            /// </summary>
            /// <param name="xdata">Asn1Node</param>
            /// <param name="index">0 based index.</param>
            /// <returns>New node index.</returns>
            int InsertChildAfter(Asn1Node xdata, int index);

            /// <summary>
            /// Insert a node in the children list after the pointed node.
            /// </summary>
            /// <param name="xdata">Asn1Node that will be instered in the children list.</param>
            /// <param name="indexNode">Index node.</param>
            /// <returns>New node index.</returns>
            int InsertChildAfter(Asn1Node xdata, Asn1Node indexNode);

            /// <summary>
            /// Remove a child from children node list by index.
            /// </summary>
            /// <param name="index">0 based index.</param>
            /// <returns>The Asn1Node just removed from the list.</returns>
            Asn1Node RemoveChild(int index);

            /// <summary>
            /// Remove the child from children node list.
            /// </summary>
            /// <param name="node">The node needs to be removed.</param>
            /// <returns></returns>
            Asn1Node RemoveChild(Asn1Node node);

            /// <summary>
            /// Get child node count.
            /// </summary>
            long ChildNodeCount { get; }

            /// <summary>
            /// Retrieve child node by index.
            /// </summary>
            /// <param name="index">0 based index.</param>
            /// <returns>0 based index.</returns>
            Asn1Node GetChildNode(int index);

            /// <summary>
            /// Get descendant node by node path.
            /// </summary>
            /// <param name="nodePath">relative node path that refer to current node.</param>
            /// <returns></returns>
            Asn1Node GetDescendantNodeByPath(string nodePath);

            /// <summary>
            /// Get/Set tag value.
            /// </summary>
            byte Tag { get; set; }

            byte MaskedTag { get; }

            /// <summary>
            /// Get tag name.
            /// </summary>
            string TagName { get; }

            /// <summary>
            /// Get data length. Not included the unused bits byte for BITSTRING.
            /// </summary>
            long DataLength { get; }

            /// <summary>
            /// Get the length field bytes.
            /// </summary>
            long LengthFieldBytes { get; }

            /// <summary>
            /// Get data offset.
            /// </summary>
            long DataOffset { get; }

            /// <summary>
            /// Get unused bits for BITSTRING.
            /// </summary>
            byte UnusedBits { get; }

            /// <summary>
            /// Get/Set node data by byte[], the data length field content and all the
            /// node in the parent chain will be adjusted.
            /// </summary>
            byte[] Data { get; set; }

            /// <summary>
            /// Get/Set parseEncapsulatedData. This property will be inherited by the
            /// child nodes when loading data.
            /// </summary>
            bool ParseEncapsulatedData { get; set; }

            /// <summary>
            /// Get the deepness of the node.
            /// </summary>
            long Deepness { get; }

            /// <summary>
            /// Get the path string of the node.
            /// </summary>
            string Path { get; }

            /// <summary>
            /// Get the node and all the descendents text description.
            /// </summary>
            /// <param name="startNode">starting node.</param>
            /// <param name="lineLen">line length.</param>
            /// <returns></returns>
            string GetText(Asn1Node startNode, int lineLen);

            /// <summary>
            /// Retrieve the node description.
            /// </summary>
            /// <param name="pureHexMode">true:Return hex string only;
            /// false:Convert to more readable string depending on the node tag.</param>
            /// <returns>string</returns>
            string GetDataStr(bool pureHexMode);

            /// <summary>
            /// Get node label string.
            /// </summary>
            /// <param name="mask">
            /// <code>
            /// SHOW_OFFSET
            /// SHOW_DATA
            /// USE_HEX_OFFSET
            /// SHOW_TAG_NUMBER
            /// SHOW_PATH</code>
            /// </param>
            /// <returns>string</returns>
            string GetLabel(uint mask);

            /// <summary>
            /// Clone a new Asn1Node by current node.
            /// </summary>
            /// <returns>new node.</returns>
            Asn1Node Clone();

            /// <summary>
            /// Clear data and children list.
            /// </summary>
            void ClearAll();
        }
    }
}