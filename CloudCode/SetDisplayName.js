// JuniorZ Exercise 2: server-side displayName validation.
// Published in Unity Cloud Code (JavaScript Scripts), environment: juniorz-test.
// Note: copy/publishing a JS file is not an automatic Cloud Code deployment.
const { DataApi } = require('@unity-services/cloud-save-1.4');

module.exports = async ({ params, context, logger }) => {
    const name = params.displayName;

    // This rule must live on the server; UI checks can be bypassed.
    if (typeof name !== 'string' || name.trim().length === 0) {
        throw new Error('INVALID_DISPLAY_NAME');
    }

    const validName = name.trim();
    const { projectId, playerId } = context;

    if (!playerId) {
        throw new Error('PLAYER_NOT_AUTHENTICATED');
    }

    // Server-owned authoritative value: player clients cannot directly write
    // Protected Player Data, even when they can write Default Player Data.
    const cloudSaveApi = new DataApi(context);
    await cloudSaveApi.setProtectedItem(projectId, playerId, {
        key: 'displayName',
        value: validName,
    });

    logger.info('Validated display name saved');
    return { success: true, displayName: validName };
};
