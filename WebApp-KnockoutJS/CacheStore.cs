/**
 * Copyright(c) 2016 CiTi Bank, Inc. All Rights Reserved.
 * 
 * 캐시(Cache) 기능를 제공한다.
 * 
 * 
 * @author: Jeongyong, Jo
 * @since : 2016-02-05
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace Citi.MyCitigoldFP.Common.Caching
{
    /// <summary>
    /// 절대 만료시간 또는 상대 만료 시간을 지정할 수 있는 캐시(Cache) 서비스.
    /// 
    /// @author: jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public sealed class CacheStore
    {
        /// <summary>
        /// 캐시 원형 모델.
        /// </summary>
        private static readonly ObjectCache _cache = MemoryCache.Default;

        /// <summary>
        /// 동기화 lock.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 기본 만료시간.
        /// </summary>
        private static readonly TimeSpan _defaultDuration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// 임시파일 (백업을 위한) 경로.
        /// </summary>
        private static string TempBackupPath = Path.GetTempPath() + "{0}.txt";

        /// <summary>
        /// 주어진 Key에 캐시된 아이템이 존재하는지 여부를 가져온다.
        /// 
        /// @author : jeongyong, Jo
        /// @date: 2016-02-12
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>아이템 존재여부.</returns>
        public static bool Exists(string key)
        {
            return _cache.Contains(key);
        }

        /// <summary>
        /// <see cref="T"/> 타입의 인스턴스를 가져온다.
        /// 
        /// @author: jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        /// <typeparam name="T">지정된 타입.</typeparam>
        /// <param name="key">Cache Key.</param>
        /// <returns><see cref="T"/>인스턴스 또는 null.</returns>
        public static T Get<T>(string key) where T : class
        {
            if (_cache.Contains(key))
                return (T)_cache.Get(key);
            return Restore<T>(key);
        }

        /// <summary>
        /// <see cref="T"/> 타입의 인스턴스를 캐시에 등록한다.
        /// 
        /// @author: jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        /// <typeparam name="T">지정된 타입.</typeparam>
        /// <param name="key">Cache Key.</param>
        /// <param name="item"><paramref name="T"/>의 인스턴스.</param>
        /// <param name="isAbsoluteExpiration">
        /// true면 현재부터 <paramref name="duration"/>(또는 기본 값 30분) 이후에 제거되고, 
        /// false인 경우 해당 아이템에 대한 엑세스가 발생하지 않는 시점부터 <paramref name="duration"/>(또는 기본 값 30분) 이후에 제거된다.
        /// </param>
        /// <param name="duration">만료시간.</param>
        /// <param name="removeCallback">캐시에 등록된 아이템이 제거될 때 호출될 메서드.</param>
        /// <param name="useBackup">백업 기능 사용여부. (기본값: false)</param>
        /// <remarks>
        /// 캐시 등록 시점에 만료로 인한 삭제에 대해 콜백 메서드 호출 기능 추가.
        /// @writer: joengyong, Jo
        /// @date: 2016-02-11
        /// 
        /// 백업/복원 기능 추가.
        /// @writer: joengyong, Jo
        /// @date: 2016-02-12
        /// </remarks>
        /// <returns>캐시에 등록된 <see cref="T"/>타입 인스턴스 아이템.</returns>
        public static T Set<T>(string key, 
            T item,
            bool isAbsoluteExpiration = false,
            TimeSpan? duration = null,
            Action<string, T> removeCallback = null,
            bool useBackup = false)
        {
            lock (_lock)
            {
                if (_cache.Contains(key))
                    _cache.Remove(key);

                var expireTime = duration ?? _defaultDuration;

                var policy = new CacheItemPolicy();
                if (isAbsoluteExpiration)
                    policy.AbsoluteExpiration = DateTime.Now.Add(expireTime);
                else
                    policy.SlidingExpiration = expireTime;

                policy.RemovedCallback = (cacheInfo) =>
                {
                    if (removeCallback != null)
                        removeCallback(cacheInfo.CacheItem.Key, (T)cacheInfo.CacheItem.Value);

                    if (cacheInfo.RemovedReason == CacheEntryRemovedReason.Expired ||
                        cacheInfo.RemovedReason == CacheEntryRemovedReason.Removed)
                        DeleteBakupItem(cacheInfo.CacheItem.Key);
                };

                _cache.Set(key, item, policy);

                if (useBackup)
                {
                    Backup(new CacheBackupModel<T>
                    {
                        Key = key,
                        Item = item,
                        IsAbsoluteExpiration = isAbsoluteExpiration,
                        ExpireTime = expireTime,
                        RemoveCallback = removeCallback
                    });
                }

                return item;
            }
        }

        /// <summary>
        /// 임시 폴더(Temp)에 캐시된 아이템을 json으로 변환한 텍스트파일을 저장한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-12
        /// </summary>
        /// <param name="backupItem">백업할 캐시 아이템 및 환경 정보 모델.</param>
        static void Backup<T>(CacheBackupModel<T> backupItem)
        {
            Task.Run(() =>
            {
                using (var sw = new StreamWriter(string.Format(TempBackupPath, backupItem.Key), false, Encoding.UTF8))
                {
                    string serializedObject = JsonConvert.SerializeObject(backupItem);

                    sw.WriteLine(serializedObject);
                }
            });
        }

        /// <summary>
        /// 백업한 파일로부터 아이템을 복원하여 캐시에 등록 후 해당 아이템을 반환한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-12
        /// </summary>
        /// <typeparam name="T">지정된 타입.</typeparam>
        /// <param name="key">key.</param>
        /// <returns>복원된 캐시 아이템 또는 <paramref name="T"/>타입의 기본 값.</returns>
        static T Restore<T>(string key)
        {
            var filePath = string.Format(TempBackupPath, key);

            if (!File.Exists(filePath))
                return default(T);

            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                try
                {
                    var backupItem = JsonConvert.DeserializeObject<CacheBackupModel<T>>(sr.ReadToEnd());

                    Set(key, 
                        backupItem.Item, 
                        backupItem.IsAbsoluteExpiration, 
                        backupItem.ExpireTime,
                        backupItem.RemoveCallback, 
                        false);

                    return backupItem.Item;
                }
                catch
                {
                    return default(T);
                }                
            }
        }

        /// <summary>
        /// 백업한 파일을 제거한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-12
        /// </summary>
        /// <param name="key">key.</param>
        static void DeleteBakupItem(string key)
        {
            var filePath = string.Format(TempBackupPath, key);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// 특정 키(key)로 등록된 캐시 아이템을 제거한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-11
        /// </summary>
        /// <param name="key">캐시 Key</param>
        public static void Remove(string key)
        {
            if (_cache.Contains(key))
            {
                lock (_lock)
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// 캐시에 등록된 모든 아이템을 제거한다.
        /// 
        /// @author: Jeongyong, jo
        /// @date: 2016-02-11
        /// </summary>
        public static void RemoveAll()
        {
            if (_cache.Any())
            {
                lock (_lock)
                {
                    foreach (var item in _cache)
                    {
                        _cache.Remove(item.Key);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 캐시된 아이템을 래핑할 모델 컨테이버
    /// </summary>
    /// <typeparam name="T">캐시 아이템 타입.</typeparam>
    public class CacheBackupModel<T>
    {
        /// <summary>
        /// 캐시 아이템 Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 캐시 아이템
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// 절대 만료처리 여부.
        /// </summary>
        public bool IsAbsoluteExpiration { get; set; }

        /// <summary>
        /// 만료시간.
        /// </summary>
        public TimeSpan ExpireTime { get; set; }

        /// <summary>
        /// 캐시 아이템 삭제시 수행할 콜백 메서드.
        /// </summary>
        [JsonIgnore]
        public Action<string, T> RemoveCallback { get; set; }
    }
}
