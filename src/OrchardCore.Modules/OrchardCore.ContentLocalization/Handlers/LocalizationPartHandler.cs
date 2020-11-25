using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly ILocalizationEntries _entries;

        public LocalizationPartHandler(ILocalizationEntries entries)
        {
            _entries = entries;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LocalizationPart part)
        {
            return context.ForAsync<CultureAspect>(cultureAspect =>
            {
                if (part.Culture != null)
                {
                    cultureAspect.Culture = CultureInfo.GetCultureInfo(part.Culture);
                }

                return Task.CompletedTask;
            });
        }

        public override async Task PublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            // Update entries from the index table after the session is committed.
            await _entries.UpdateEntriesAsync();
        }

        public override Task UnpublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            // Update entries from the index table after the session is committed.
            return _entries.UpdateEntriesAsync();
        }

        public override Task RemovedAsync(RemoveContentContext context, LocalizationPart part)
        {
            if (context.NoActiveVersionLeft)
            {
                // Indicate to the index provider that the related content item was removed.
                part.ContentItemRemoved = true;

                // Update entries from the index table after the session is committed.
                return _entries.UpdateEntriesAsync();
            }

            return Task.CompletedTask;
        }

        public override Task CloningAsync(CloneContentContext context, LocalizationPart part)
        {
            var clonedPart = context.CloneContentItem.As<LocalizationPart>();
            clonedPart.LocalizationSet = String.Empty;
            clonedPart.Apply();
            return Task.CompletedTask;
        }
    }
}
