using System;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;

namespace Hik.Communication.Scs.Communication.EndPoints
{
    ///<summary>
    /// Represents a server side end point in SCS.
    ///</summary>
    public abstract class ScsEndPoint : IEquatable<ScsEndPoint>
    {
        /// <summary>
        /// Create a Scs End Point from a string.
        /// Address must be formatted as: protocol://address
        /// For example: tcp://89.43.104.179:10048 for a TCP endpoint with
        /// IP 89.43.104.179 and port 10048.
        /// </summary>
        /// <param name="endPointAddress">Address to create endpoint</param>
        /// <returns>Created end point</returns>
        public static ScsEndPoint CreateEndPoint(string endPointAddress)
        {
            //Check if end point address is null
            if (string.IsNullOrEmpty(endPointAddress))
            {
                throw new ArgumentNullException("endPointAddress");
            }

            //If not protocol specified, assume TCP.
            var endPointAddr = endPointAddress;
            if (!endPointAddr.Contains("://"))
            {
                endPointAddr = "tcp://" + endPointAddr;
            }

            //Split protocol and address parts
            var splittedEndPoint = endPointAddr.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedEndPoint.Length != 2)
            {
                throw new ApplicationException(endPointAddress + " is not a valid endpoint address.");
            }

            //Split end point, find protocol and address
            var protocol = splittedEndPoint[0].Trim().ToLower();
            var address = splittedEndPoint[1].Trim();
            switch (protocol)
            {
                case "tcp":
                    return new ScsTcpEndPoint(address);
                default:
                    throw new ApplicationException("Unsupported protocol " + protocol + " in end point " + endPointAddress);
            }
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        internal abstract IScsServer CreateServer();

        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        internal abstract IScsClient CreateClient();

        #region added by longqinsi 20140508
        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server and is bound to a specific local port.
        /// </summary>
        /// <param name="localPort">The loacl port that the client is bound to.</param>
        /// <returns>Scs Client</returns>
        internal abstract IScsClient CreateClient(int localPort);
        #endregion

        /// <summary>This mothed is implemented by derived classes to verify whether equal to another SceEndPoint.</summary>
        /// <param name="other">The other ScsEndPoint to compare to.</param>
        /// <returns>Where equal to.</returns>
        public abstract bool Equals(ScsEndPoint other);

        /// <summary>
        /// Overrides base.Equals
        /// </summary>
        /// <param name="obj">The other object to compare to.</param>
        /// <returns>Whether equal to the other object.</returns>
        public override bool Equals(object obj)
        {
            var otherScsEndPoint = obj as ScsEndPoint;
            if(object.ReferenceEquals(otherScsEndPoint, null))
            {
                return false;
            }
            return Equals(otherScsEndPoint);
        }

        /// <summary>
        /// Overrides base.GetHaseCode
        /// </summary>
        /// <returns>base.GetHashCode()</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Checks whether two ScsEndPoint objects are equal.
        /// </summary>
        /// <param name="a">ScsEndPoint a</param>
        /// <param name="b">ScsEndPoint b</param>
        /// <returns>whether two ScsEndPoint objects are equal.</returns>
        public static bool operator ==(ScsEndPoint a, ScsEndPoint b)
        {
            bool aIsNull = object.ReferenceEquals(a, null);
            bool bIsNull = object.ReferenceEquals(b, null);
            if(aIsNull ^ bIsNull)//a == null && b != null || a != null && b == null
            {
                return false;
            }
            else
            {
                if(aIsNull)//a == null && b == null
                {
                    return true;
                }
                else//a != null && b != null
                {
                    return a.Equals(b);
                }
            }
        }

        /// <summary>
        /// Checks whether two ScsEndPoint objects are not equal.
        /// </summary>
        /// <param name="a">ScsEndPoint a</param>
        /// <param name="b">ScsEndPoint b</param>
        /// <returns>whether two ScsEndPoint objects are not equal.</returns>
        public static bool operator !=(ScsEndPoint a, ScsEndPoint b)
        {
            return !(a == b);
        }
    }
}