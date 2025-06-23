import { getSettings as storageGetSettings, setForceStreamingGpt41 } from '@/entities/settings/api/settings-storage';
import { query, action, revalidate } from '@solidjs/router';

export const getSettings = query(async () => {
    'use server';
    return await storageGetSettings();
}, 'globalSettings');

export const updateForceStreaming = action(async (enabled: boolean) => {
    'use server';
    await setForceStreamingGpt41(enabled);
}, 'updateForceStreaming');

export const refetchSettings = () => revalidate(getSettings.key); 