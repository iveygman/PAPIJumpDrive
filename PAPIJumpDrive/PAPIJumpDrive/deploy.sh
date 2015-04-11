set -e

DEBUG=$1
PARTS_PATH=../../PartAssets
KSPCUSTOMSOUNDS=$(cd "$(dirname "$PARTS_PATH")"; pwd)/KSPCustomSounds/
PARTS_PATH=$(cd "$(dirname "$PARTS_PATH")"; pwd)/PartAssets
KSP_PATH="/Users/iveygman/Library/Application Support/Steam/SteamApps/common/Kerbal Space Program"
BUILT_DLL_PATH=bin/$DEBUG/PAPIJumpDrive.dll
DEST_DIR=$KSP_PATH"/GameData/PiconAdvanced/"

rm -r "$(echo $DEST_DIR)"
mkdir -p "$(echo $DEST_DIR)"
cp -r "$(echo $PARTS_PATH)"/* "$(echo $DEST_DIR)"
cp "$(echo $BUILT_DLL_PATH)" "$(echo $DEST_DIR)"
cp -r "$(echo $KSPCUSTOMSOUNDS)" "$(echo $KSP_PATH/GameData)"
