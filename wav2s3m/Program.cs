using System.Security.Cryptography;
using wav2s3m;

static string CalculateMD5(string filename)
{
    using (var md5 = MD5.Create())
    {
        using (var stream = File.OpenRead(filename))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}

S3M s3M = new S3M("test.s3m");
s3M.save("out.s3m");

if (CalculateMD5("test.s3m") == CalculateMD5("out.s3m"))
{
    Console.WriteLine("Correct Implementation");
}
else
{
    Console.WriteLine("ERROR");
}