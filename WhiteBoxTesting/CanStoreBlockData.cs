using Xunit;
using HayaiFE.Models;

namespace HayaiFE.Tests
{
    public class BlockDataTests
    {
        [Fact]
        public void BlockData_CanStoreSubjectAndSeatNumbers()
        {
            var block = new BlockData
            {
                BlockNumber = 5,
                Subject = "Cyber Security 410244C",
                Branch = "Computer",
                RoomNo = "M301",
                BlockFloor = 3,
                StartingSeatNumber = "B1906904201",
                EndingSeatNumber = "B1906904230"
            };

            Assert.Equal("Cyber Security 410244C", block.Subject);
            Assert.Equal("B1906904230", block.EndingSeatNumber);
        }
    }
}
