using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Server.Sampling
{
    public class TailSamplingProcessor : BaseProcessor<Activity>
    {
        private readonly string _statusTagName;

        public TailSamplingProcessor(string statusTagName = "otel.status_code")
        {
            _statusTagName = statusTagName;
        }

        public override void OnEnd(Activity activity)
        {
            // If the activity is already sampled, we don't need to do anything
            if (activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded))
            {
                return;
            }

            // Check if this is an error activity
            bool isError = false;

            if (activity.Status == ActivityStatusCode.Error)
            {
                isError = true;
            }
            else if (activity.TagObjects != null)
            {
                foreach (var tag in activity.TagObjects)
                {
                    if (tag.Key == _statusTagName)
                    {
                        if (tag.Value?.ToString() == "ERROR")
                        {
                            isError = true;
                            break;
                        }
                    }
                }
            }

            if (isError)
            {
                activity.ActivityTraceFlags |= ActivityTraceFlags.Recorded;
            }
        }
    }
}
