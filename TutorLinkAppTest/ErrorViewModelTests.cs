using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
        {
            var model = new ErrorViewModel { RequestId = null };

            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmpty_ReturnsFalse()
        {
            var model = new ErrorViewModel { RequestId = "" };

            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsWhitespace_ReturnsFalse()
        {
            var model = new ErrorViewModel { RequestId = "   " };

            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdHasValue_ReturnsTrue()
        {
            var model = new ErrorViewModel { RequestId = "12345" };

            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsGuid_ReturnsTrue()
        {
            var model = new ErrorViewModel { RequestId = "abc-123-def-456" };

            Assert.True(model.ShowRequestId);
        }
    }
}
