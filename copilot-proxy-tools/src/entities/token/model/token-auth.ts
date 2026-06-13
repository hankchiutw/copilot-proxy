import { createSignal } from 'solid-js';
import type { TokenAuth } from './types';

const [tokenAuth, setTokenAuth] = createSignal<TokenAuth>(null);
export { tokenAuth };
export async function generateToken() {
  setTokenAuth({ message: 'Generating token...' });
  const res = await fetch('/admin/token', {
    method: 'POST',
  });
  const reader = res.body.pipeThrough(new TextDecoderStream()).getReader();

  while (true) {
    const { value, done } = await reader.read();
    if (done) break;
    const json = JSON.parse(value);
    setTokenAuth(json);
  }
}
