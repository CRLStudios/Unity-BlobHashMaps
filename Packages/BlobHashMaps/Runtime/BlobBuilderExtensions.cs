/*
 * Written by Bart van de Sande
 * https://github.com/bartofzo/BlobHashMaps
 * https://bartvandesande.nl
 */

#if ENABLE_UNITY_COLLECTIONS_CHECKS
#define BLOBHASHMAP_SAFE
#endif

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace CRL.BlobHashMaps
{
    /// <summary>
    /// Extension methods for BlobBuilder to allocate BlobHashMaps with
    /// </summary>
    public static class BlobBuilderExtensions
    {
        // 16384 is somewhat arbitrary but tests have shown that for small enough capacities this will
        // be a bit faster while still not allocating loads of memory
        private const int UseBucketCapacityRatioOfThreeUpTo = 16384;
        
        /// <summary>
        /// Allocates a BlobHashMap and copies all key value pairs from the source NativeHashMap
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="source">Source hashmap to copy keys and values from</param>
        public static void ConstructHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobHashMap<TKey, TValue> blobHashMap, ref NativeParallelHashMap<TKey, TValue> source)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            int count = source.Count();
            var kv = source.GetKeyValueArrays(Allocator.Temp);
            var hashMapBuilder = builder.AllocateHashMap(ref blobHashMap, count);
                
            for (int i = 0; i < kv.Length; i++)
                hashMapBuilder.Add(kv.Keys[i], kv.Values[i]);
        }
        
        /// <summary>
        /// Allocates a BlobHashMap and copies all key value pairs from the source NativeHashMap
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="source">Source hashmap to copy keys and values from</param>
        public static void ConstructHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobHashMap<TKey, TValue> blobHashMap, ref NativeHashMap<TKey, TValue> source)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            int count = source.Count;
            var kv = source.GetKeyValueArrays(Allocator.Temp);
            var hashMapBuilder = builder.AllocateHashMap(ref blobHashMap, count);
                
            for (int i = 0; i < kv.Length; i++)
                hashMapBuilder.Add(kv.Keys[i], kv.Values[i]);
        }
        
        /// <summary>
        /// Allocates a BlobHashMap and copies all key value pairs from the source dictionary
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="source">Source hashmap to copy keys and values from</param>
        public static void ConstructHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobHashMap<TKey, TValue> blobHashMap, Dictionary<TKey, TValue> source)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            int count = source.Count;
            int ratio = count <= UseBucketCapacityRatioOfThreeUpTo ? 3 : 2;
            
            var hashMapBuilder = builder.AllocateHashMap(ref blobHashMap, source.Count, ratio);
            foreach (var kv in source)
                hashMapBuilder.Add(kv.Key, kv.Value);
        }
        
        /// <summary>
        /// Allocates a BlobHashMap and returns a builder than can be used to add values manually
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="capacity">Capacity of the allocated hashmap. This value cannot be changed after allocation</param>
        /// <returns>Builder that can be ued to add values to the hashmap</returns>
        public static BlobBuilderHashMap<TKey, TValue> AllocateHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobHashMap<TKey, TValue> blobHashMap, int capacity)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return AllocateHashMap(ref builder, ref blobHashMap, capacity, capacity <= UseBucketCapacityRatioOfThreeUpTo ? 3 : 2);
        }
        
        /// <summary>
        /// Allocates a BlobHashMap and returns a builder than can be used to add values manually
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="capacity">Capacity of the allocated hashmap. This value cannot be changed after allocation</param>
        /// <param name="bucketCapacityRatio">
        /// Bucket capacity ratio to use when allocating the hashmap.
        /// A higher value may result in less collisions and slightly better performance, but memory consumption increases exponentially.
        /// </param>
        /// <returns>Builder that can be ued to add values to the hashmap</returns>
        public static BlobBuilderHashMap<TKey, TValue> AllocateHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobHashMap<TKey, TValue> blobHashMap, 
            int capacity, int bucketCapacityRatio)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            var hashmapBuilder = new BlobBuilderHashMap<TKey, TValue>(capacity, bucketCapacityRatio, ref builder, ref blobHashMap.data);
            return hashmapBuilder;
        }
        
        /// <summary>
        /// Allocates a BlobHashMap and copies all key value pairs from the source NativeHashMap
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobMultiHashMap">Reference to the struct BlobMultiHashMap field</param>
        /// <param name="source">Source multihashmap to copy keys and values from</param>
        public static void ConstructMultiHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobMultiHashMap<TKey, TValue> blobMultiHashMap, ref NativeParallelMultiHashMap<TKey, TValue> source)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            int count = source.Count();

            var kv = source.GetKeyValueArrays(Allocator.Temp);
            var hashMapBuilder = builder.AllocateMultiHashMap(ref blobMultiHashMap, count);
                
            for (int i = 0; i < kv.Length; i++)
                hashMapBuilder.Add(kv.Keys[i], kv.Values[i]);
        }
        
        /// <summary>
        /// Allocates a BlobMultiHashMap and returns a builder than can be used to add values manually
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobMultiHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="capacity">Capacity of the allocated multihashmap. This value cannot be changed after allocation</param>
        /// <returns>Builder that can be ued to add values to the multihashmap</returns>
        public static BlobBuilderMultiHashMap<TKey, TValue> AllocateMultiHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobMultiHashMap<TKey, TValue> blobMultiHashMap, int capacity)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            var hashmapBuilder = new BlobBuilderMultiHashMap<TKey, TValue>(capacity, capacity <= UseBucketCapacityRatioOfThreeUpTo ? 3 : 2, ref builder, ref blobMultiHashMap.data);
            return hashmapBuilder;
        }
        
        /// <summary>
        /// Allocates a BlobMultiHashMap and returns a builder than can be used to add values manually
        /// </summary>
        /// <param name="builder">Reference to the struct BlobBuilder used to construct the hashmap</param>
        /// <param name="blobMultiHashMap">Reference to the struct BlobHashMap field</param>
        /// <param name="capacity">Capacity of the allocated multihashmap. This value cannot be changed after allocation</param>
        /// <param name="bucketCapacityRatio">
        /// Bucket capacity ratio to use when allocating the hashmap.
        /// A higher value may result in less collisions and slightly better performance, but memory consumption increases exponentially.
        /// </param>
        /// <returns>Builder that can be ued to add values to the multihashmap</returns>
        public static BlobBuilderMultiHashMap<TKey, TValue> AllocateMultiHashMap<TKey, TValue>(
            this ref BlobBuilder builder, ref BlobMultiHashMap<TKey, TValue> blobMultiHashMap, 
            int capacity, int bucketCapacityRatio)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            var hashmapBuilder = new BlobBuilderMultiHashMap<TKey, TValue>(capacity, bucketCapacityRatio, ref builder, ref blobMultiHashMap.data);
            return hashmapBuilder;
        }
    }
}