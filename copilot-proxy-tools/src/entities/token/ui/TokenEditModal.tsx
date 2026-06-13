import { renameToken } from '@/entities/token/model/token-item';
import type { TokenItem } from '@/entities/token/model/types';
import { useAction } from '@solidjs/router';
import type { Component } from 'solid-js';
import { createEffect, createSignal } from 'solid-js';
import type { Accessor } from 'solid-js/types/server/reactive.js';

const TokenEditModal: Component<{ editingItem: Accessor<TokenItem>; onClose: () => void }> = ({
  editingItem,
  onClose,
}) => {
  let editModal: HTMLDialogElement | undefined;
  let inputRef: HTMLInputElement;
  const [nameInput, setNameInput] = createSignal('');
  const renameTokenAction = useAction(renameToken);

  createEffect(() => {
    if (editingItem()) {
      setNameInput(editingItem().name);
      editModal.showModal();
      setTimeout(() => inputRef.focus(), 100);
    }
  });
  const onClickSave = () => {
    renameTokenAction(editingItem().id, nameInput());
    onClickClose();
  };
  const onClickClose = () => {
    if (editModal) {
      editModal.close();
    }
  };
  return (
    <dialog ref={editModal} class="d-modal" onClose={onClose}>
      <div class="d-modal-box">
        <h3 class="text-lg font-bold">Edit token</h3>
        <fieldset class="d-fieldset">
          <legend class="d-fieldset-legend">Name</legend>
          <input
            ref={inputRef}
            type="text"
            class="d-input"
            placeholder="Type here"
            value={nameInput()}
            onInput={(e) => setNameInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && onClickSave()}
          />
        </fieldset>
        <div class="d-modal-action">
          <form method="dialog">
            <button type="button" class="d-btn d-btn-soft d-btn-primary mr-1" onClick={onClickSave}>
              Save
            </button>
            <button type="button" class="d-btn d-btn-ghost" onClick={onClickClose}>
              Cancel
            </button>
          </form>
        </div>
      </div>
      <form method="dialog" class="d-modal-backdrop">
        <button type="button" onClick={onClickClose}>
          Close
        </button>
      </form>
    </dialog>
  );
};

export default TokenEditModal;
