import { useState } from "react";
import styles from "./Shared.module.css";

function AccessPassDisplay({ accessPass }) {
  const [isClicked, setIsClicked] = useState(false);

  const copyToClipboard = () => {
    navigator.clipboard.writeText(accessPass).catch((err) => {
      console.error("Error copying text to clipboard: ", err);
    });

    setIsClicked((prev) => true);
  };

  return (
    <div className={styles["shared-container"]}>
      <p className={styles["shared-paragraph"]}>Jūsų vienkartinis kodas</p>
      <div className={styles["shared-code-container"]}>
        <div className={styles["shared-code"]}>{accessPass}</div>
      </div>
      <button
        className={styles[`${isClicked ? "shared-btn-clicked" : "shared-btn"}`]}
        type="button"
        onClick={copyToClipboard}
      >
        {isClicked ? "Nukopijuota" : "Kopijuoti"}
      </button>
    </div>
  );
}

export default AccessPassDisplay;
