import { getSettings, updateForceStreaming, refetchSettings } from '@/entities/settings/model/settings';
import { createAsync, useAction } from '@solidjs/router';
import type { Component } from 'solid-js';
import type { GlobalSettings } from '@/entities/settings/api/settings-storage';

const SettingsPanel: Component = () => {
    const settings = createAsync<GlobalSettings>(() => getSettings());
    const updateStreamingAction = useAction(updateForceStreaming);

    const onToggle = (e: Event) => {
        const target = e.target as HTMLInputElement;
        updateStreamingAction(target.checked);
        refetchSettings();
    };

    return (
        <div class="border-b border-zinc-700 pb-4 mb-4">
            <label class="flex items-center cursor-pointer select-none">
                <input
                    type="checkbox"
                    checked={settings()?.forceStreamingGpt41 ?? false}
                    onChange={onToggle}
                    class="checkbox checkbox-sm mr-2"
                />
                <span class="text-sm">Force streaming for gpt-4.1 models</span>
            </label>
        </div>
    );
};

export default SettingsPanel; 