import Copy from 'lucide-solid/icons/copy';
import CopyCheck from 'lucide-solid/icons/copy-check';
import type { Component } from 'solid-js';
import { createSignal } from 'solid-js';
import { Dynamic } from 'solid-js/web';

type IconCopyProps = {
  tooltips?: string;
  textToCopy?: () => string | undefined;
};

const IconCopy: Component<IconCopyProps> = ({ tooltips = '', textToCopy }) => {
  const [copied, setCopied] = createSignal(false);
  const Icon = () => (copied() ? CopyCheck : Copy);

  const handleClick = () => {
    if (!textToCopy) return;
    navigator.clipboard.writeText(textToCopy() || '');
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  return (
    <div
      class="d-tooltip d-tooltip-right inline-block align-middle ml-2 cursor-pointer hover:bg-neutral-700 active:bg-neutral-600 transition-colors duration-200 rounded p-1"
      data-tip={copied() ? 'Copied!' : tooltips}
      onClick={handleClick}
      onKeyPress={handleClick}
    >
      <Dynamic component={Icon()} size={18} />
    </div>
  );
};

export default IconCopy;
