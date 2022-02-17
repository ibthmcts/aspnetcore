// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OutputCaching.Policies;
public class PredicatePolicy : IOutputCachingRequestPolicy
{
    // TODO: Accept a non async predicate too?

    private readonly Func<IOutputCachingContext, Task<bool>> _predicate;
    private readonly IOutputCachingRequestPolicy _policy;

    public PredicatePolicy(Func<IOutputCachingContext, Task<bool>> predicate, IOutputCachingRequestPolicy policy)
    {
        _predicate = predicate;
        _policy = policy;
    }

    public Task OnRequestAsync(IOutputCachingContext context)
    {
        if (_predicate == null)
        {
            return _policy.OnRequestAsync(context);
        }

        var task = _predicate(context);

        if (task.IsCompletedSuccessfully)
        {
            if (task.Result)
            {
                return _policy.OnRequestAsync(context);
            }

            return Task.CompletedTask;
        }

        return Awaited(task, _policy, context);

        async static Task Awaited(Task<bool> task, IOutputCachingRequestPolicy policy, IOutputCachingContext context)
        {
            if (await task)
            {
                await policy.OnRequestAsync(context);
            }
        }
    }
}
