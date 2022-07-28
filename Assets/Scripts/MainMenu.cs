using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private static readonly string address = "0x3cE643dc61bb40bB0557316539f4A93016051b81";

    public enum Network
    {
        Polygon,
        Mumbai
    }

    public Network network = Network.Polygon;

    public string accountName = "<your-account-name-here>";

    public string projectName = "<your-project-name-here>";

    public Button button;

    public Text label;

    private void Awake()
    {
        button.onClick.AddListener(ConnectWallet);
    }

    private async void ConnectWallet()
    {
        label.text = "Verifying...";
        button.enabled = false;

        try
        {
            var verified = await Verify();
            if (verified)
            {
                label.text = "Loading...";
                SceneManager.LoadScene(1);
            }
            else
            {
                label.text = "Invalid account!";
                button.enabled = true;
            }
        } catch (Exception ex)
        {
            label.text = ex.Message;
            button.enabled = true;
        }
    }

    private async Task<bool> Verify()
    {
        // sign a message to prove ownership
        var message = "Valist License verification";
        var signature = await Web3Wallet.Sign(message);

        // EC recover the signer account
        var signer = new EthereumMessageSigner();
        var account = signer.EncodeUTF8AndEcRecover(message, signature);

        if (network == Network.Polygon)
        {
            // generate project id
            var chainId = "0x0000000000000000000000000000000000000000000000000000000000000089";
            var accountId = GenerateId(chainId, accountName);
            var projectId = GenerateId(accountId, projectName);
            var tokenId = HexToDecimalString(projectId);

            // check license balance
            var balance = await ERC1155.BalanceOf("polygon", "mainnet", address, account, tokenId);
            return !balance.IsZero;
        }
        else if (network == Network.Mumbai)
        {
            // generate project id
            var chainId = "0x0000000000000000000000000000000000000000000000000000000000013881";
            var accountId = GenerateId(chainId, accountName);
            var projectId = GenerateId(accountId, projectName);
            var tokenId = HexToDecimalString(projectId);

            // check license balance
            var balance = await ERC1155.BalanceOf("polygon", "testnet", address, account, tokenId);
            return !balance.IsZero;
        }

        return false;
    }

    private string HexToDecimalString(string hex)
    {
        var number = HexBigIntegerConvertorExtensions.HexToBigInteger(hex, false);
        return number.ToString();
    }

    private string GenerateId(string parentId, string name)
    {
        var nameHash = Sha3Keccack.Current.CalculateHash(name);
        return Sha3Keccack.Current.CalculateHashFromHex(parentId, nameHash);
    }
}
