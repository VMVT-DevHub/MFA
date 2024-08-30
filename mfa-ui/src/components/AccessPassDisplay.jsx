import styles from "./Shared.module.css";

function AccessPassDisplay({ accessPass }) {
  const copyToClipboard = () => {
    navigator.clipboard.writeText(accessPass).catch((err) => {
      console.error("Error copying text to clipboard: ", err);
    });
  };

  return (
    <div className={styles["shared-container"]}>
      <p className={styles["shared-paragraph"]}>Jūsų vienkartinis kodas</p>
      <div className={styles["shared-code-container"]}>
        <div className={styles["shared-code"]}>{accessPass}</div>
      </div>
      <button
        className={styles["shared-btn"]}
        type="button"
        onClick={copyToClipboard}
      >
        Kopijuoti
      </button>
    </div>
  );
}

export default AccessPassDisplay;
