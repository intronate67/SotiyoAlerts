using Discord;
using Discord.Interactions;
using Moq;
using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Modules;
using SotiyoAlerts.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotiyoAlerts.Test.Modules
{
    public class FilterModuleTest
    {
        private readonly Mock<IInteractionContext> _contextMock = new();
        private readonly Mock<IDiscordInteraction> _interactionMock = new();
        private readonly Mock<IChannelFilterService> _channelFilterService;
        private readonly Mock<FilterModule> _moduleMock;

        public FilterModuleTest()
        {
            _channelFilterService = new Mock<IChannelFilterService>();
            _moduleMock = new Mock<FilterModule>(() => new FilterModule(_channelFilterService.Object));
            _contextMock.SetupGet(x => x.Interaction).Returns(_interactionMock.Object);
            ((IInteractionModuleBase)_moduleMock.Object).SetContext(_contextMock.Object);
        }

        // sa-add

        [Fact]
        public async Task AddTracking_NoExisting()
        {
            // Arrange
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetFiltersForChannelId(It.IsAny<long>())).Returns(new HashSet<long>());

            // Act
            await _moduleMock.Object.AddTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetFiltersForChannelId(It.IsAny<long>()), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), It.IsAny<MessageComponent>(), It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task AddTracking_WithExisting()
        {
            // Arrange 
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetFiltersForChannelId(It.IsAny<long>())).Returns(new HashSet<long>() { 1L });

            // Act
            await _moduleMock.Object.AddTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetFiltersForChannelId(It.IsAny<long>()), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), It.IsAny<MessageComponent>(), It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        // sa-start

        [Fact]
        public async Task StartTracking_NoFilters()
        {
            // Arrange 
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.StartTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.EnableChannelFilter(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StartTracking_AllActive()
        {
            // Arrange 
            SetupMockChannel();
            var channelFilter = ChannelFilter
                .Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.StartTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.EnableChannelFilter(channelFilter), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StartTracking_SomeToEnable()
        {
            // Arrange 
            SetupMockChannel();
            var channelFilters = new HashSet<ChannelFilter>
            {
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe())
            };

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(channelFilters);

            // Act
            await _moduleMock.Object.StartTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.EnableChannelFilter(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), It.IsAny<MessageComponent>(), It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StartTracking_OneToEnable()
        {
            // Arrange 
            SetupMockChannel();

            var channelFilter = ChannelFilter
                .Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.StartTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.EnableChannelFilter(channelFilter), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        // sa-stop

        [Fact]
        public async Task StopTracking_NoFilters()
        {
            // Arrange 
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.StopTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.DisableChannelFilter(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StopTracking_AllDisabled()
        {
            // Arrange 
            SetupMockChannel();
            var channelFilter = ChannelFilter
                .Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.StopTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.DisableChannelFilter(channelFilter), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StopTracking_SomeToDisable()
        {
            // Arrange 
            SetupMockChannel();
            var channelFilters = new HashSet<ChannelFilter>
            {
                ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe())
            };

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(channelFilters);

            // Act
            await _moduleMock.Object.StopTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.DisableChannelFilter(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), It.IsAny<MessageComponent>(), It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task StopTracking_OneToDisable()
        {
            // Arrange 
            SetupMockChannel();

            var channelFilter = ChannelFilter
                .Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.StopTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.DisableChannelFilter(channelFilter), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        // sa-delete

        [Fact]
        public async Task DeleteTracking_NoExistingOrAllDeleted()
        {
            // Arrange
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.DeleteTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.MarkChannelFilterDeleted(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTracking_SomeToDelete()
        {
            // Arrange 
            SetupMockChannel();
            var channelFilters = new HashSet<ChannelFilter>
            {
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe()),
                ChannelFilter.Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe())
            };

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(channelFilters);

            // Act
            await _moduleMock.Object.DeleteTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.DisableChannelFilter(It.IsAny<ChannelFilter>()), Times.Never);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), It.IsAny<MessageComponent>(), It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTracking_OneToDelete()
        {
            // Arrange 
            SetupMockChannel();

            var channelFilter = ChannelFilter
                .Create(0, 0, 0, false, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.DeleteTracking();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _channelFilterService.Verify(x => x.MarkChannelFilterDeleted(channelFilter), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        // sa-info

        [Fact]
        public async Task GetChannelFilter_NoFilters_CurrentChannel()
        {
            // Arrange
            SetupMockChannel();
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.GetChannelFilter(null);

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, null,
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetChannelFilter_ExistingFilters_CurrentChannel()
        {
            // Arrange
            SetupMockChannel();
            SetupMockUser();
            var channelFilter = ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());
            
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.GetChannelFilter(null);

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(It.IsAny<long>()), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetChannelFilter_NoFilters_SpecifiedChannel()
        {
            // Arrange
            var channelMock = SetupMockChannel();
            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.GetChannelFilter(channelMock.Object);

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(Convert.ToInt64(channelMock.Object.Id)), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, null,
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetChannelFilter_ExistingFilters_SpecifiedChannel()
        {
            // Arrange
            var channelMock = SetupMockChannel();
            SetupMockUser();
            var channelFilter = ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());

            _channelFilterService.Setup(x => x.GetChannelFilters(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.GetChannelFilter(channelMock.Object);

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFilters(Convert.ToInt64(channelMock.Object.Id)), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        // sa-info-all

        [Fact]
        public async Task GetGuildFilters_NoFilters()
        {
            // Arrange
            SetupMockChannel();
            var guild = SetupMockGuild();
            _channelFilterService.Setup(x => x.GetChannelFiltersForGuild(It.IsAny<long>())).Returns(new HashSet<ChannelFilter>());

            // Act
            await _moduleMock.Object.GetChannelFilter();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFiltersForGuild(Convert.ToInt64(guild.Object.Id)), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, null,
                It.IsAny<RequestOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetGuildFilters_ExistingFilters()
        {
            // Arrange
            SetupMockChannel();
            SetupMockUser();
            var guild = SetupMockGuild();
            var channelFilter = ChannelFilter.Create(0, 0, 0, true, false, DateTime.Now, Filter.CreateFilterUnsafe(), Channel.CreateChannelUnsafe());

            _channelFilterService.Setup(x => x.GetChannelFiltersForGuild(It.IsAny<long>()))
                .Returns(new HashSet<ChannelFilter>() { channelFilter });

            // Act
            await _moduleMock.Object.GetChannelFilter();

            // Assert
            _channelFilterService.Verify(x => x.GetChannelFiltersForGuild(Convert.ToInt64(guild.Object.Id)), Times.Once);
            _interactionMock
                .Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed[]>(), It.IsAny<bool>(), true,
                It.IsAny<AllowedMentions>(), null, It.IsAny<Embed>(),
                It.IsAny<RequestOptions>()), Times.Once);
        }

        private Mock<IMessageChannel> SetupMockChannel()
        {
            var channel = new Mock<IMessageChannel>();
            _contextMock.SetupGet(x => x.Channel).Returns(channel.Object);
            return channel;
        }

        private Mock<IGuildUser> SetupMockUser()
        {
            var guildUser = new Mock<IGuildUser>();
            _contextMock.SetupGet(x => x.User).Returns(guildUser.Object);
            return guildUser;
        }

        private Mock<IGuild> SetupMockGuild()
        {
            var guild = new Mock<IGuild>();
            _contextMock.SetupGet(x => x.Guild).Returns(guild.Object);
            return guild;
        }
    }
}
