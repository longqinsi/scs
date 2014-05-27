using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication.Messages;
using Hik.Communication.ScsServices.Communication.Messengers;
using Hik.Protobuf;
using Hik.Utility;

namespace Hik.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class is used to generate a dynamic proxy to invoke remote methods.
    /// It translates method invocations to messaging.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    internal class RemoteInvokeProxy<TProxy, TMessenger> : RealProxy where TMessenger : IMessenger
    {
        /// <summary>
        /// Messenger object that is used to send/receive messages.
        /// </summary>
        private readonly RMIRequestReplyMessenger<TMessenger> _clientMessenger;

        /// <summary>
        /// Creates a new RemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        public RemoteInvokeProxy(RMIRequestReplyMessenger<TMessenger> clientMessenger)
            : base(typeof(TProxy))
        {
            _clientMessenger = clientMessenger;
        }

        /// <summary>
        /// Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            var message = msg as IMethodCallMessage;
            if (message == null)
            {
                return null;
            }

            Type[] parameterTypes = (Type[])msg.Properties["__MethodSignature"];
            var requestMessage = new ScsRemoteInvokeMessage
            {
                ServiceName = TypeNameConverter.Default.ConvertToTypeName(typeof (TProxy)),
                MethodName = message.MethodName,
                ParameterTypeNames = Array.ConvertAll(parameterTypes, TypeNameConverter.Default.ConvertToTypeName),
                Parameters = message.Args != null ? message.Args.Select(a => ArbitraryObject.CreateArbitraryObject(a)).ToArray() : null
            };

            var responseMessage = _clientMessenger.SendMessageAndWaitForResponse(requestMessage) as ScsRemoteInvokeReturnMessage;
            if (responseMessage == null)
            {
                return null;
            }

            if (responseMessage.RemoteException != null)
            {
                return new ReturnMessage(responseMessage.RemoteException, message);
            }
            else
            {
                object ret = ArbitraryObject.GetValue(responseMessage.ReturnValue);
                object[] outArgs = (responseMessage.OutArguments != null && responseMessage.OutArguments.Length > 0) ? Array.ConvertAll(responseMessage.OutArguments, ArbitraryObject.GetValue) : null;
                object[] args;
                int argCount;
                if(outArgs != null && outArgs.Length > 0)
                {
                    args = new object[message.ArgCount];
                    argCount = message.ArgCount;
                    for (int i = 0; i < outArgs.Length; i++)
                    {
                        int argIndex = responseMessage.OutArgumentIndices[i];
                        if (responseMessage.OutArgumentRefFlags[i])
                        {
                            Type parameterType = null;
                            var origArg = message.GetArg(argIndex);
                            if (origArg != null)
                            {
                                parameterType = origArg.GetType();
                            }
                            if (parameterType != null && parameterType.IsClass)
                            {
                                ArbitraryObject.MergeObject(responseMessage.OutArguments[i], ref origArg);
                                args[argIndex] = origArg;
                                continue;
                            }
                        }
                        args[argIndex] = ArbitraryObject.GetValue(responseMessage.OutArguments[i]);
                    }
                }
                else
                {
                    args = null;
                    argCount = 0;
                }
                return new ReturnMessage(ret, args, argCount, message.LogicalCallContext, message);
            }
        }
    }
}