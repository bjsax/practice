using System;
using System.IO;
using System.Collections.Generic;
using MD.IO.Buffer;

namespace MD.BoltReader
{
    public class Bucket
    {
        public string BucketName { get; set; }
        public long PageId { get; set; }

        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private Dictionary<string, Bucket> subBucket = new Dictionary<string, Bucket>();

        public Bucket(string bucketName, long pgId)
        {
            BucketName = bucketName;
            PageId = pgId;
        }

        public void SetkeyValuePairs(string keyName, string valueName)
        {
            if (!keyValuePairs.ContainsKey(keyName))
                keyValuePairs.Add(keyName, valueName);
        }

        public void SetSubBucket(string bucketName, long pgId)
        {
            var subBucket = new Bucket(bucketName, pgId);
            this.subBucket.Add(bucketName, subBucket);
        }

        public List<string> GetKeys()
        {
            return new List<string>(keyValuePairs.Keys);
        }

        public string GetValueOf(string keyName)
        {
            return keyValuePairs.ContainsKey(keyName)
                ? keyValuePairs[keyName]
                : null;
        }

        public List<string> GetSubBucketNames()
        {
            return new List<string>(subBucket.Keys);
        }

        public Bucket GetSubBucketOf(string bucketName)
        {
            return subBucket.ContainsKey(bucketName)
                ? subBucket[bucketName]
                : null;
        }
    }

    public enum PageFlag
    {
        Leaf = 0x02,
        Branch = 0x01
    }

    public enum LeafFlag
    {
        KeyValuePair = 0x00,
        SubBucket = 0x01
    }

    public class BoltDB
    {
        const int metaPageN = 2; // MetaPage<1>, MetaPage<2>
        const int inlineBucketID = 0;

        private Bucket root;
        public bool IsValid { get; private set; }

        public BoltDB(string fileName)
        {
            FileStream fs;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                fs = null;
            }

            IsValid = init(fs);
        }
        
        public Bucket GetRoot()
        {
            return IsValid ? root : null;
        }

        public void PrintSubBucket(Bucket bucket)
        {
            if (!IsValid)
                return;

            Console.WriteLine("Bucket : " + bucket.BucketName);

            if (bucket.GetSubBucketNames().Count != 0)
            {
                foreach (var name in bucket.GetSubBucketNames())
                    Console.WriteLine("- subBucket : " + name);
            }
            else
            {
                Console.WriteLine("no subBucket");
            }

            Console.WriteLine();
        }

        public void PrintSubBucketList(Bucket rootBucket)
        {
            if (!IsValid)
                return;

            PrintSubBucket(rootBucket);

            if (rootBucket.GetSubBucketNames().Count != 0)
            {
                foreach (var name in rootBucket.GetSubBucketNames())
                    PrintSubBucketList(rootBucket.GetSubBucketOf(name));
            }
        }

        public void PrintLeafNode(Bucket bucket)
        {
            if (!IsValid)
                return;

            Console.WriteLine("Bucket : " + bucket.BucketName);

            if (bucket.GetKeys().Count != 0)
            {
                foreach (var key in bucket.GetKeys())
                    Console.WriteLine("(" + key + ") " + "(" + bucket.GetValueOf(key) + ")");
            }
            else
            {
                Console.WriteLine("KeyValue is empty");
            }

            if (bucket.GetSubBucketNames().Count != 0)
            {
                foreach (var name in bucket.GetSubBucketNames())
                    Console.WriteLine("subBucket : " + name);
            }
            else
            {
                Console.WriteLine("no subBucket");
            }

            Console.WriteLine();
        }

        public void PrintAll(Bucket rootBucket)
        {
            if (!IsValid)
                return;

            PrintLeafNode(rootBucket);

            if (rootBucket.GetSubBucketNames().Count != 0)
            {
                foreach (var name in rootBucket.GetSubBucketNames())
                    PrintAll(rootBucket.GetSubBucketOf(name));
            }
        }

        private bool init(FileStream fs)
        {
            if (fs == null)
                return false;
            
            try
            {
                if (checkMagicNumbers(fs))
                    root = build(fs, root);
                else
                    return false;
            }
            catch
            {
                root = null;
                return false;
            }
            finally
            {
                fs.Close();
            }

            return true;
        }

        private bool checkMagicNumbers(FileStream fs)
        {
            var magicNumbers = new byte[] { 0xED, 0xDA, 0x0C, 0xED };

            for (var i = 0; i < metaPageN; i++)
            {
                var magicIndex = (long)(0x1000 * i + 0x10);
                var buffer = readPrefix(fs, at: magicIndex);

                for (var j = 0; j < magicNumbers.Length; j++)
                {
                    if (magicNumbers[j] != buffer[j])
                        return false;
                }
            }

            return true;
        }

        private Bucket build(FileStream fs, Bucket root)
        {
            var rootHeader = readPrefix(fs, at: 0x20); // rootHeader in MetaPage<1>
            var rootPageId = getBucketPgId(from: rootHeader) * 0x1000;
            root = new Bucket("Root", rootPageId); // tempBucket

            reconstruct(fs, root);

            return root;
        }
       
        private byte[] readPrefix(FileStream fs, long at, int length=0x10)
        {
            var buffer = new byte[length];
            fs.Seek(at, SeekOrigin.Begin);
            fs.Read(buffer, 0, length);

            return buffer;
        }

        private void reconstruct(FileStream fs, Bucket bucket, long seekPoint= -1)
        {
            var offset = seekPoint == -1
                ? bucket.PageId
                : seekPoint;

            var pageHeader = readPrefix(fs, at: offset);
            var pageInfo = getPageInfo(from: pageHeader); // pageFlag, itemCount

            var itemCount = pageInfo["itemCount"];
            var cursor = offset + 0x10;

            if (pageInfo["pageFlag"] == (short)PageFlag.Leaf) // leaf page
            {
                for (var i = 0; i < itemCount; i++)
                {
                    var leafHeader = readPrefix(fs, at: cursor);
                    analyzeLeafNode(fs, bucket, leafHeader, cursor);
                    cursor += 0x10;
                }
            }
            else if (pageInfo["pageFlag"] == (short)PageFlag.Branch) // branch page
            {
                for (var i = 0; i < itemCount; i++) 
                {
                    var branchHeader = readPrefix(fs, at: cursor);
                    bucket.PageId = getBranchPgId(from: branchHeader) * 0x1000;
                    reconstruct(fs, bucket);
                    cursor += 0x10;
                }
            }
        }
        
        private void analyzeLeafNode(FileStream fs, Bucket bucket, byte[] leafHeader, long cursor)
        {
            var leafInfo = getLeafInfo(from: leafHeader);

            var offset = cursor + leafInfo["position"];
            var length = leafInfo["keySize"] + leafInfo["valueSize"];

            var buffer = readPrefix(fs, at: offset, length);
            
            var bb = new ByteBuffer2(buffer, 0, leafInfo["keySize"]);

            if (leafInfo["leafFlag"] == (int)LeafFlag.KeyValuePair)
            {
                var keyName = bb.GetStringUTF8(leafInfo["keySize"]);
                var valueName = bb.GetStringUTF8(leafInfo["valueSize"]);

                bucket.SetkeyValuePairs(keyName, valueName);
            }
            else if (leafInfo["leafFlag"] == (int)LeafFlag.SubBucket)
            {
                var subBucketName = bb.GetStringUTF8(leafInfo["keySize"]);
                var subBucketHeader = bb.GetBytes(0x10);
                var subBucketPgId = getBucketPgId(from: subBucketHeader);
                
                bucket.SetSubBucket(subBucketName, subBucketPgId * 0x1000);

                long seekPoint = -1; // split or branch

                if (subBucketPgId == inlineBucketID) // inlineBucketID = 0
                    seekPoint = (long)(cursor + leafInfo["position"] + leafInfo["keySize"] + 0x10);

                reconstruct(fs, bucket.GetSubBucketOf(subBucketName), seekPoint);
            }
        }

        private Dictionary<string,short> getPageInfo(byte[] from) // from Page Header
        {
            var bb = new ByteBuffer2(from, 8, 2);

            return new Dictionary<string, short>
            {
                { "pageFlag", bb.GetInt16LE() },
                { "itemCount", bb.GetInt16LE() }
            };
        }

        private long getBucketPgId(byte[] from) // from Bucket Header
        {
            var bb = new ByteBuffer2(from, 0, 8);

            return bb.GetInt64LE();
        }

        private Dictionary<string, int> getLeafInfo(byte[] from) // from Leaf Header
        {
            var bb = new ByteBuffer2(from, 0, 4);

            return new Dictionary<string, int> {
                { "leafFlag", bb.GetInt32LE() },
                { "position", bb.GetInt32LE() },
                { "keySize", bb.GetInt32LE() },
                { "valueSize", bb.GetInt32LE() }
            };
        }

        private long getBranchPgId(byte[] from) // from Branch Header
        {
            var bb = new ByteBuffer2(from, 8, 8);

            return bb.GetInt64LE();
        }
    }
}