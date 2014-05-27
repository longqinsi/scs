using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.ScsServices.Communication.Messages;

namespace Hik.Collections
{
    /// <summary>
    /// 用于protobuf-net的类型缓存
    /// </summary>
    public class TypeCache
    {
        #region 单例模式
        private static int isInitialized = 0;
        internal static void Initialize()
        {
            if (Interlocked.Exchange(ref isInitialized, 1) == 0)
            {
                if (_singleton == null)
                {
                    _singleton = new TypeCache();
                }
            }
        }

        private static TypeCache _singleton;

        /// <summary>类型缓存的单例实例</summary>
        public static TypeCache Singleton
        {
            get
            {
                return _singleton;
            }
        }
        #endregion

        private ReaderWriterLockSlim _SyncObject = new ReaderWriterLockSlim();
        private readonly BiDictionary<Type, int> cache;

        /// <summary>供本程序集内的类型使用的编号范围:1-1000</summary>
        private int internalID = 0;
        /// <summary>供外部类型使用的编号范围从1001开始</summary>
        private int externalID = 1000;


        /// <summary>创建缓存的默认实例</summary>
        private TypeCache()
        {
            cache = new BiDictionary<Type, int>();
            #region 缓存内部类型。注意：以下语句的顺序不能改变，如果要添加新的类型，请添加到最后，否则程序可能出错。
            AddInternalType<Hik.Communication.Scs.Communication.Messages.ScsPingMessage>();
            AddInternalType<Hik.Communication.Scs.Communication.Messages.ScsTextMessage>();
            AddInternalType<Hik.Communication.Scs.Communication.Messages.ScsRawDataMessage>();
            AddInternalType<Hik.Communication.ScsServices.Communication.Messages.ScsRemoteInvokeMessage>();
            AddInternalType<Hik.Communication.ScsServices.Communication.Messages.ScsRemoteInvokeReturnMessage>();
            AddInternalType<Hik.Communication.ScsServices.Communication.Messages.ScsRemoteRegisterMessage>();
            AddInternalType<Hik.Communication.ScsServices.Communication.Messages.ScsRemoteUnregisterMessage>();
            AddInternalType<Hik.Communication.Scs.Communication.CommunicationException>();
            AddInternalType<Hik.Communication.ScsServices.Communication.Messages.ScsRemoteException>();
            //在最后添加新的类型
            #endregion
        }

        /// <summary>添加内部类型到缓存</summary>
        /// <typeparam name="T">待添加类型</typeparam>
        private void AddInternalType<T>()
        {
            cache[typeof(T)] = internalID++;
        }

        /// <summary>添加类型到缓存(如果缓存中已有该类型则直接跳过)</summary>
        /// <param name="type">待添加类型</param>
        /// <returns>缓存自身</returns>
        public TypeCache AddType(Type type)
        {
            _SyncObject.EnterWriteLock();
            try
            {
                if (!cache.ContainsKey(type))
                {
                    cache[type] = externalID++;
                }
            }
            finally
            {
                _SyncObject.ExitWriteLock();
            }
            return this;
        }

        /// <summary>把一组类型添加到缓存</summary>
        /// <param name="types">待添加类型列表</param>
        /// <returns>缓存自身</returns>
        public TypeCache AddTypes(IEnumerable<Type> types)
        {
            if (types != null)
            {
                _SyncObject.EnterWriteLock();
                try
                {
                    foreach (var type in types)
                    {
                        if (!cache.ContainsKey(type))
                        {
                            cache[type] = externalID++;
                        }
                    }
                }
                finally
                {
                    _SyncObject.ExitWriteLock();
                }
            }
            return this;
        }

        /// <summary>添加多个类型到缓存</summary>
        /// <param name="types">待添加类型</param>
        /// <returns>缓存自身</returns>
        public TypeCache AddTypes(params Type[] types)
        {
            if (types != null && types.Length > 0)
            {
                AddTypes(types as IEnumerable<Type>);
            }
            return this;
        }

        //public bool ContainsType(Type type)
        //{
        //    _SyncObject.EnterReadLock();
        //    try
        //    {
        //        return cache.ContainsKey(type);
        //    }
        //    finally
        //    {
        //        _SyncObject.ExitReadLock();
        //    }

        /// <summary>获取类型的ID</summary>
        /// <param name="type">类型</param>
        /// <returns>类型ID;如果类型不在缓存中，则返回-1</returns>
        public int GetIDByType(Type type)
        {
            _SyncObject.EnterReadLock();
            try
            {
                int id;
                if (cache.TryGetValue(type, out id))
                {
                    return id;
                }
                else
                {
                    return -1;
                }
            }
            finally
            {
                _SyncObject.ExitReadLock();
            }
        }

        /// <summary>根据类型ID获取类型</summary>
        /// <param name="id">类型ID</param>
        /// <returns>类型ID对应的类型;如果不存在则返回null</returns>
        public Type GetTypeByID(int id)
        {
            _SyncObject.EnterReadLock();
            try
            {
                Type type;
                if (cache.TryGetKey(id, out type))
                {
                    return type;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                _SyncObject.ExitReadLock();
            }
        }
    }
}
