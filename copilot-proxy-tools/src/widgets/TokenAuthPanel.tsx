import { tokenAuth } from '@/entities/token/model/token-auth';
import { refetchTokenList } from '@/entities/token/model/token-item';
import IconCopy from '@/shared/ui/IconCopy';
import type { Component } from 'solid-js';
import { Match, Suspense, Switch, createEffect } from 'solid-js';

const VerificationInfo: Component = () => {
  return (
    <span>
      Please visit&nbsp;
      <a
        href={tokenAuth().verificationUri}
        target="_blank"
        rel="noopener noreferrer"
        class="underline text-blue-400"
      >
        {tokenAuth().verificationUri}
      </a>
      &nbsp; and paste the code:&nbsp;
      <span class="font-mono font-bold">{tokenAuth().userCode}</span>
      <IconCopy tooltips="Copy code" textToCopy={() => tokenAuth().userCode} />
    </span>
  );
};

const AccessTokenInfo: Component = () => {
  return (
    <span>
      Keep the token safe. You will not be able to see it again. Your access token is:&nbsp;
      <span class="font-mono font-bold">{tokenAuth().accessToken}</span>
      <IconCopy tooltips="Copy access token" textToCopy={() => tokenAuth().accessToken} />
    </span>
  );
};

const TokenAuthPanel: Component = () => {
  createEffect(() => {
    if (tokenAuth()?.accessToken) {
      refetchTokenList();
    }
  });
  return (
    <Suspense fallback={<p class="text-sm">Waiting...</p>}>
      <Switch>
        <Match when={tokenAuth()}>
          <p class="text-sm mb-4">{tokenAuth().message}</p>
          <p class="text-sm mb-4">
            {tokenAuth().verificationUri && tokenAuth().deviceCode && <VerificationInfo />}
            {tokenAuth().accessToken && <AccessTokenInfo />}
          </p>
        </Match>
      </Switch>
    </Suspense>
  );
};

export default TokenAuthPanel;
