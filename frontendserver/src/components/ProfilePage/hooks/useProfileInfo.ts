// Custom hooks for fetching reservation and user data
import { useState, useEffect } from 'react';
import api from '../../../services/api.ts';
import { GroupedReservation, ReservationDto, UserDto } from '../../../types/reservation.types.ts';

/**
 * Hook to fetch active reservations grouped by reservation group
 */
export const useActiveReservations = (userId: string | undefined) => {
  const [reservations, setReservations] = useState<GroupedReservation[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchReservations = async () => {
    if (!userId) {
      setReservations([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await api.get(`reservations/my-reservations/grouped/${userId}`);
      if (response.status === 200) {
        setReservations(response.data);
      }
    } catch (err: any) {
      console.error('Failed to fetch active reservations:', err);
      const errorMessage = err.response?.data?.error || err.response?.data || err.message || 'Failed to load reservations';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReservations();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  return { reservations, loading, error, refetch: fetchReservations };
};

/**
 * Hook to fetch reservation history (cancelled and past reservations)
 */
export const useReservationHistory = (userId: string | undefined) => {
  const [history, setHistory] = useState<ReservationDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchHistory = async () => {
    if (!userId) {
      setHistory([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await api.get(`reservations/my-reservations/history/${userId}`);
      if (response.status === 200) {
        setHistory(response.data);
      }
    } catch (err: any) {
      console.error('Failed to fetch reservation history:', err);
      const errorMessage = err.response?.data?.error || err.response?.data || err.message || 'Failed to load history';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchHistory();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  return { history, loading, error, refetch: fetchHistory };
};

/**
 * Hook to fetch user profile data dynamically
 */
export const useUserProfile = (userId: string | undefined) => {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = async () => {
    if (!userId) {
      setUser(null);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Fetch all users and find the one with matching ID
      const response = await api.get('users/get-users');
      if (response.status === 200) {
        const users: UserDto[] = response.data;
        const foundUser = users.find(u => u.id === userId);
        if (foundUser) {
          setUser(foundUser);
        } else {
          setError('User not found');
        }
      }
    } catch (err: any) {
      console.error('Failed to fetch user profile:', err);
      const errorMessage = err.response?.data?.error || err.response?.data || err.message || 'Failed to load user profile';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUser();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  return { user, loading, error, refetch: fetchUser };
};

