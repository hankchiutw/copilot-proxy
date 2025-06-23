import storage from 'node-persist';

const STORAGE_DIR = process.env.STORAGE_DIR || '.storage';

const settingsStorage = storage.create({
    dir: `${STORAGE_DIR}/settings`,
    ttl: false,
});

await settingsStorage.init();

export interface GlobalSettings {
    forceStreamingGpt41: boolean;
}

const DEFAULT_SETTINGS: GlobalSettings = {
    forceStreamingGpt41: false,
};

export async function getSettings(): Promise<GlobalSettings> {
    const stored = await settingsStorage.getItem('global-settings');
    if (!stored) return DEFAULT_SETTINGS;
    return { ...DEFAULT_SETTINGS, ...stored } as GlobalSettings;
}

export async function setForceStreamingGpt41(enabled: boolean): Promise<void> {
    const current = await getSettings();
    await settingsStorage.setItem('global-settings', {
        ...current,
        forceStreamingGpt41: enabled,
    });
} 