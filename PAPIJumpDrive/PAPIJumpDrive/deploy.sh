set -e

DEBUG=$1
PARTS_PATH=../../PartAssets
PARTS_PATH=$(cd "$(dirname "$PARTS_PATH")"; pwd)/PartAssets
KSP_PATH="/Users/iveygman/Library/Application Support/Steam/SteamApps/common/Kerbal Space Program"
BUILT_DLL_PATH=bin/$DEBUG/PAPIJumpDrive.dll
DEST_DIR=$KSP_PATH"/GameData/PiconAdvanced/"
echo $KSP_PATH
echo $DEST_DIR
echo $BUILT_DLL_PATH
echo $PARTS_PATH
rm -r "$(echo $DEST_DIR)"
mkdir -p "$(echo $DEST_DIR)"
cp -r "$(echo $PARTS_PATH)"/* "$(echo $DEST_DIR)"
cp "$(echo $BUILT_DLL_PATH)" "$(echo $DEST_DIR)"