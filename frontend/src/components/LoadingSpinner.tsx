import { useEffect, useState } from "react";

const SLOW_THRESHOLD_MS = 4000;

const LoadingSpinner = () => {
  const [showSlowMessage, setShowSlowMessage] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setShowSlowMessage(true), SLOW_THRESHOLD_MS);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="flex flex-col items-center justify-center gap-3 py-12">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-gray-200 border-t-orange-500" />
      {showSlowMessage && (
        <p className="text-sm text-gray-400">
          Waking up the server, this can take up to a minute...
        </p>
      )}
    </div>
  );
};

export default LoadingSpinner;
