import {
  getTokenList,
  refreshTokenMeta,
  removeToken,
  setDefaultToken,
} from '@/entities/token/model/token-item';
import type { TokenItem } from '@/entities/token/model/types';
import QuotaInfo from '@/entities/token/ui/QuotaInfo';
import TokenAddModal from '@/entities/token/ui/TokenAddModal';
import TokenEditModal from '@/entities/token/ui/TokenEditModal';
import { createAsync, useAction } from '@solidjs/router';
import Bookmark from 'lucide-solid/icons/bookmark';
import BookmarkCheck from 'lucide-solid/icons/bookmark-check';
import Pencil from 'lucide-solid/icons/pencil';
import Trash from 'lucide-solid/icons/trash-2';
import type { Component } from 'solid-js';
import { ErrorBoundary, For, createSignal } from 'solid-js';

type MenuItemProps = {
  children?: any;
  tooltip: string;
  onClick: any;
};

const MenuItem: Component<MenuItemProps> = (props: MenuItemProps) => {
  return (
    <div
      class="d-tooltip d-tooltip-top ml-1 cursor-pointer hover:bg-neutral-700 active:bg-neutral-600 transition-colors duration-200 rounded p-1"
      onClick={props.onClick}
      onKeyPress={props.onClick}
      data-tip={props.tooltip}
    >
      <span class="opacity-60 text-zinc-400">{props.children}</span>
    </div>
  );
};

const TokenList: Component = () => {
  const tokenList = createAsync(getTokenList);
  const [editingItem, setEditingItem] = createSignal(null);
  const [isAdding, setIsAdding] = createSignal(false);

  const removeTokenAction = useAction(removeToken);
  const refreshTokenMetaAction = useAction(refreshTokenMeta);
  const setDefaultTokenAction = useAction(setDefaultToken);

  const showModal = (item: TokenItem) => {
    setEditingItem(item);
  };

  const onClickDelete = (item: TokenItem) => {
    if (window.confirm(`Are you sure to delete the token ${item.name}?`)) {
      removeTokenAction(item.id);
    }
  };

  const onClickDefault = (item: TokenItem) => {
    setDefaultTokenAction(item.id);
  };

  const onClickRefresh = (item: TokenItem) => {
    refreshTokenMetaAction(item.id);
  };

  return (
    <ul class="d-list bg-base-100 rounded-box shadow-md">
      <TokenEditModal editingItem={editingItem} onClose={() => setEditingItem(null)} />
      <TokenAddModal isOpen={isAdding} onClose={() => setIsAdding(false)} />
      <div
        onClick={() => setIsAdding(true)}
        onKeyPress={() => setIsAdding(true)}
        class="d-tooltip d-tooltip-top my-3 text-center rounded-sm text-xs hover:bg-zinc-700 cursor-pointer active:bg-zinc-600 transition-colors duration-200"
      >
        ＋
      </div>
      <ErrorBoundary fallback={<div>Something went wrong</div>}>
        <For each={tokenList()}>
          {(item) => (
            <>
              <li class="d-list-row border rounded-lg mb-4 border-zinc-600">
                <div class="d-list-col-grow">
                  <div class="flex items-center">
                    <div class="text-blue-500 flex-1">{item.name}</div>
                    <MenuItem onClick={() => showModal(item)} tooltip="Edit">
                      <Pencil size={18} />
                    </MenuItem>
                    <MenuItem
                      onClick={() => onClickDefault(item)}
                      tooltip={item.default ? 'Default token' : 'Set as default'}
                    >
                      {item.default ? (
                        <BookmarkCheck size={18} class="text-emerald-400" />
                      ) : (
                        <Bookmark size={18} />
                      )}
                    </MenuItem>
                    <MenuItem onClick={() => onClickDelete(item)} tooltip="Delete">
                      <Trash size={18} class="text-rose-400" />
                    </MenuItem>
                  </div>
                  <div class="text-zinc-400">{item.token}</div>
                  <div class="text-zinc-500 text-xs my-2">
                    Created at: {new Date(item.createdAt).toLocaleString()}
                  </div>
                </div>
                <QuotaInfo item={item} onClickRefresh={() => onClickRefresh(item)} />
              </li>
            </>
          )}
        </For>
      </ErrorBoundary>
    </ul>
  );
};

export default TokenList;
