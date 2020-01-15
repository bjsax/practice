using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using MD.IO.Buffer;

namespace KeyValueDB
{
    class Bucket
    {
        string BucketName;
        long pgID;
        Dictionary<string, string> keyValues = new Dictionary<string, string>();
        Dictionary<string, Bucket> subBucket = new Dictionary<string, Bucket>();

        public Bucket(string bucketName, long pgID)
        {
            this.BucketName = bucketName;
            this.pgID = pgID * 0x1000;
        }

        public long GetPageID()
        {
            return this.pgID;
        }

        public void SetPageID(long pgID)
        {
            this.pgID = pgID * 0x1000;
        }

        public void SetKeyValues(string key, string value)
        {
            if(!keyValues.ContainsKey(key))
                this.keyValues.Add(key, value);
        }

        public void SetSubBucket(string bucketName, long pgID)
        {
            var subBucket = new Bucket(bucketName, pgID);
            this.subBucket.Add(bucketName, subBucket);
        }

        public string[] GetKeys()
        {
            if (keyValues.Count == 0)
                return null;

            var keys = new string[keyValues.Count];
            var i = 0;
            
            foreach(var key in keyValues.Keys)
            {
                keys[i] = key;
                i++;
            }

            return keys;
        }

        public string GetValueOf(string key)
        {
            if (keyValues.ContainsKey(key))
                return keyValues[key];
            else
                return null;
        }

        public string GetBucketName()
        {
            return BucketName;
        }

        public string[] GetSubBucketNames()
        {
            if (subBucket.Count == 0)
                return null;

            var names = new string[subBucket.Count];
            var i = 0;

            foreach (var name in subBucket.Keys)
            {
                names[i] = name;
                i++;
            }

            return names;
        }

        public Bucket GetSubBucketOf(string name)
        {
            if (subBucket.ContainsKey(name))
                return subBucket[name];
            else
                return null;
        }
    }

    class BoltDB
    {
        FileStream fs;
        Bucket root;

        public BoltDB(string fileName)
        {
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                fs = null;
            }
        }

        public bool IsValid()
        {
            if (fs == null)
                return false;

            byte[] magic = new byte[] { 0xED, 0xDA, 0x0C, 0xED };

            for (long i = 0; i < 2; i++) //MetaPage<1>, MetaPage<2>의 magic number를 비교
            {
                var buffer = readPrefix(0x1000 * i + 0x10);

                for (var j = 0; j < 4; j++) //magic number 4자리 비교
                {
                    if (!(magic[j] == buffer[j])) 
                    {
                        fs.Close();
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Build()
        {
            try
            {
                var buffer = readPrefix(at: 0x20); // BucketHeader in MetaPage<1>
                root = new Bucket("root", readbucketHeader(buffer)); // 사용자가 만든 Bucket이 아니라 임의의 Bucket
                traverse(root);
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
            }

            return true;
        } 

        public Bucket GetRootBucket()
        {
            return root;
        }

        public void PrintLeafNode(Bucket bucket)
        {
            Console.WriteLine("Bucket : " + bucket.GetBucketName());

            if (bucket.GetKeys() != null)
            {
                foreach (var key in bucket.GetKeys())
                    Console.WriteLine("(" + key + ") " + "(" + bucket.GetValueOf(key) + ")");
            }
            else
            {
                Console.WriteLine("KeyValue is empty");
            }
            
            if (bucket.GetSubBucketNames() != null)
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

        public void PrintAll(Bucket root)
        {
            PrintLeafNode(root);

            if (root.GetSubBucketNames() != null)
            {
                foreach (var name in root.GetSubBucketNames())
                {
                    PrintAll(root.GetSubBucketOf(name));
                }
            }
        }

        private byte[] readPrefix(long at, int length=0x10)
        {
            var buffer = new byte[length];
            fs.Seek(at, SeekOrigin.Begin);
            fs.Read(buffer, 0, length);

            return buffer;
        }

        private void traverse(Bucket bucket, long seekPoint= -1)
        {
            var offset = seekPoint == -1
                ? bucket.GetPageID()
                : seekPoint;

            var buffer = readPrefix(at: offset);
            var pageInf = readpageHeader(buffer); // pageflag, itemcount
            var cursor = offset + 0x10;

            if (pageInf.Item1 == 0x02) // leaf page
            {
                for (var i = 0; i < pageInf.Item2; i++) // item count 만큼 반복해서 읽기
                {
                    buffer = readPrefix(at: cursor);
                    readLeafNode(bucket, buffer, cursor);
                    cursor += 0x10;
                }
            }
            else if (pageInf.Item1 == 0x01) // branch page : 어차피 정보는 leaf에 모두 모이기 때문에 페이지를 넘겨주기만 하면 된다
            {
                for (var i = 0; i < pageInf.Item2; i++) // item count 만큼 반복해서 읽기
                {
                    buffer = readPrefix(at: cursor);
                    bucket.SetPageID(readbranchHeader(buffer));
                    traverse(bucket);
                    cursor += 0x10;
                }
            }
        }
        
        private void readLeafNode(Bucket bucket, byte[] buffer, long cursor)
        {
            var leafInf = readleafHeader(buffer);
            var buffer2 = readPrefix(at: cursor + leafInf[1], length: leafInf[2] + leafInf[3]);
            
            var byteBuffer = new ByteBuffer2(buffer2, 0x00, leafInf[2]);

            if (leafInf[0] == 0) // leaf가 key-value인 경우
            {
                bucket.SetKeyValues(byteBuffer.GetStringUTF8(leafInf[2]), byteBuffer.GetStringUTF8(leafInf[3]));
            }
            else if (leafInf[0] == 1) //leaf가 subBucket인 경우
            {
                var subBucketName = byteBuffer.GetStringUTF8(leafInf[2]);
                var subBucketPageID = readbucketHeader(byteBuffer.GetBytes(0x0010));
                
                bucket.SetSubBucket(subBucketName, subBucketPageID);
                
                if (subBucketPageID != 0) //split된 경우 or branch page로 가는 경우
                {
                    traverse(bucket.GetSubBucketOf(subBucketName));
                }
                else    //inline bucket인 경우
                {
                    traverse(bucket.GetSubBucketOf(subBucketName), cursor + leafInf[1] + leafInf[2] + 0x0010);  //같은 page에서 Traverse
                }
            }
        }

        private (short, short) readpageHeader(byte[] buffer)
        {
            var byteBuffer = new ByteBuffer2(buffer, 0x0008, 0x0002);
            var pageFlag = byteBuffer.GetInt16LE();
            var count = byteBuffer.GetInt16LE();

            return (pageFlag, count);
        }

        private long readbucketHeader(byte[] buffer)
        {
            var byteBuffer = new ByteBuffer2(buffer, 0x0000, 0x0008);
            var BucketPageID = byteBuffer.GetInt64LE();

            return BucketPageID;
        }

        private int[] readleafHeader(byte[] buffer)
        {
            var byteBuffer = new ByteBuffer2(buffer, 0x0000, 0x0004);
            var leafInf = new int[4];

            for (var i = 0; i < 4; i++)
                leafInf[i] = byteBuffer.GetInt32LE(); // section, position, keysize, valuesize

            return leafInf;
        }

        private long readbranchHeader(byte[] buffer)
        {
            var byteBuffer = new ByteBuffer2(buffer, 0x0008, 0x0008);
            var branchPageID = byteBuffer.GetInt64LE();

            return branchPageID;
        }
    }

    class MainClass
    {
        static void Main(string[] args)
        {
            var fileName = @"C:\Workspace\src\depth3.db";
            var bolt = new BoltDB(fileName);

            if (!bolt.IsValid())
                return;

            if (!bolt.Build())
                return;

            var root = bolt.GetRootBucket();

            foreach(var name in root.GetSubBucketNames())
            {
                Console.WriteLine(name);
            }

            if (root.GetKeys() != null)
            {
                foreach (var key in root.GetKeys())
                {
                    Console.WriteLine(root.GetValueOf(key));
                }
            }

            bolt.PrintLeafNode(root);
            
            bolt.PrintAll(root);
        }
    }
}