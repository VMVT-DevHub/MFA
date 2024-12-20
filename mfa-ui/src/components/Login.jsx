import { useState } from "react";
import styles from "./Shared.module.css";
import exclamationMark from "../assets/exclamationmark.svg";

function Login({ onSubmit, isError }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const [isDisabled, setIsDisabled] = useState(false)

  return (
    <form
      className={styles["shared-container"]}
      onSubmit={async (e) => {
        e.preventDefault();

        let trimmedUsername = username;
        if(username.includes("@vmvt.lt"))
        {
          trimmedUsername = username.split("@vmvt.lt")[0];
        }

        await onSubmit(trimmedUsername, password);
      }}
      autoComplete="off"
    >
      <p className={styles["shared-paragraph"]}>
        Prisijunkite su savo kompiuterio slaptažodžiu
      </p>
      <input
        className={styles["shared-input"]}
        type="text"
        placeholder="Prisijungimo vardas"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
        required
        autoComplete="off"
        readOnly
        onFocus={(e) => e.target.removeAttribute("readonly")}
      />
      <input
        className={styles["shared-input-psw"]}
        type="text"
        placeholder="Slaptažodis"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
        autoComplete="off"
        readOnly
        onFocus={(e) => e.target.removeAttribute("readonly")}
      />
      {isError && (
        <div className={styles["shared-error-container"]}>
          <img
            src={exclamationMark}
            alt="Error"
            className={styles["shared-error-icon"]}
          />
          <p className={styles["shared-error-text"]}>
            Neteisingi prisijungimo duomenys
          </p>
        </div>
      )}
      <button
        className={styles["shared-btn"]}
        disabled={isDisabled}
        onClick={async (e) => {
          e.preventDefault();
          setIsDisabled(true);
          await onSubmit(username, password);
          setIsDisabled(false);
        }}
      >
        Prisijungti
      </button>
    </form>
  );
}

export default Login;
